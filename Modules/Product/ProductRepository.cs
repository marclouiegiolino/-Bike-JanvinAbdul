using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Product
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        public ProductRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT
                p.product_id,
                p.category_id,
                p.brand,
                p.product_name,
                p.description,
                p.image_url,
                p.status,
                p.created_at
            FROM products p
            LEFT JOIN categories c ON c.category_id = p.category_id
        ";

        private Product MapReader(DbDataReader reader)
        {
            return new Product
            {
                ProductId   = ReadValue(reader, "product_id",   0L),
                CategoryId  = ReadValue(reader, "category_id",  0L),
                Brand       = ReadValue(reader, "brand",        string.Empty),
                ProductName = ReadValue(reader, "product_name", string.Empty),
                Description = ReadValue(reader, "description",  string.Empty),
                ImgUrl      = ReadValue(reader, "image_url",    string.Empty),
                Status      = ReadValue(reader, "status",       "active"),
                CreatedAt   = ReadValue(reader, "created_at",   string.Empty),
            };
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            string sql = BaseSelect + " ORDER BY p.product_id, p.category_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Product?> GetByIdAsync(long id)
        {
            string sql = BaseSelect + " WHERE p.product_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task<List<Product>> GetByCategoryIdAsync(long categoryId)
        {
            string sql = BaseSelect + " WHERE p.category_id = @categoryId ORDER BY p.product_name";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("categoryId", categoryId) });
            return items.ToList();
        }

        public async Task AddAsync(Product entity)
        {
            string sql = @"
                INSERT INTO products (category_id, brand, product_name, description, image_url, status, created_at)
                VALUES (@category_id, @brand, @product_name, @description, @image_url, @status, @created_at)
                RETURNING product_id";

            string status = string.IsNullOrWhiteSpace(entity.Status) ? "active" : entity.Status;
            string createdAt = string.IsNullOrWhiteSpace(entity.CreatedAt)
                ? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                : entity.CreatedAt;

            long? newId = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("category_id",  entity.CategoryId),
                CreateParameter("brand",        entity.Brand),
                CreateParameter("product_name", entity.ProductName),
                CreateParameter("description",  entity.Description),
                CreateParameter("image_url",    entity.ImgUrl),
                CreateParameter("status",       status),
                CreateParameter("created_at",   createdAt),
            });

            entity.ProductId = newId.GetValueOrDefault();
            entity.Status = status;
            entity.CreatedAt = createdAt;
        }

        public async Task UpdateAsync(Product entity)
        {
            string sql = @"
                UPDATE products
                SET category_id  = @category_id,
                    brand        = @brand,
                    product_name = @product_name,
                    description  = @description,
                    image_url    = @image_url,
                    status       = @status,
                    created_at   = @created_at
                WHERE product_id = @product_id";

            string status = string.IsNullOrWhiteSpace(entity.Status) ? "active" : entity.Status;

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("category_id",  entity.CategoryId),
                CreateParameter("brand",        entity.Brand),
                CreateParameter("product_name", entity.ProductName),
                CreateParameter("description",  entity.Description),
                CreateParameter("image_url",    entity.ImgUrl),
                CreateParameter("status",       status),
                CreateParameter("created_at",   entity.CreatedAt),
                CreateParameter("product_id",   entity.ProductId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM products WHERE product_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> ExistsAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM products WHERE product_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<Product>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null)
        {
            string sql;
            string countSql;
            var queryParams = new List<DbParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search.Trim()}%";
                sql = BaseSelect + @"
                    WHERE p.product_name LIKE @search OR p.brand LIKE @search OR p.description LIKE @search
                    ORDER BY p.product_name
                    LIMIT @size OFFSET @offset";
                countSql = @"
                    SELECT COUNT(*) FROM products p
                    WHERE p.product_name LIKE @search OR p.brand LIKE @search OR p.description LIKE @search";

                queryParams.Add(CreateParameter("search", pattern));
            }
            else
            {
                sql = BaseSelect + @"
                    ORDER BY p.product_name
                    LIMIT @size OFFSET @offset";
                countSql = "SELECT COUNT(*) FROM products";
            }

            queryParams.Add(CreateParameter("size", pageSize));
            queryParams.Add(CreateParameter("offset", (pageNumber - 1) * pageSize));

            var items = await ExecuteReaderToListAsync(sql, MapReader, queryParams.ToArray());

            DbParameter[]? totalParams = !string.IsNullOrWhiteSpace(search)
                ? new[] { CreateParameter("search", $"%{search.Trim()}%") }
                : null;

            int total = await ExecuteScalarAsync<int>(countSql, totalParams);

            return new PaginationModel<Product>
            {
                Items       = items.ToList(),
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}
