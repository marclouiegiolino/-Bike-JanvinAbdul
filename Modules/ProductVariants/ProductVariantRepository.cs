using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.ProductVariants
{
    public class ProductVariantRepository : BaseRepository, IProductVariantRepository
    {
        public ProductVariantRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT
                v.variant_id,
                v.product_id,
                v.sku,
                v.frame_size,
                v.color,
                v.wheel_size,
                v.price,
                v.stock_quantity,
                v.is_active
            FROM product_variants v
            LEFT JOIN products pr ON pr.product_id = v.product_id
        ";

        private ProductVariant MapReader(DbDataReader reader)
        {
            return new ProductVariant
            {
                VariantId     = ReadValue(reader, "variant_id",     0L),
                ProductId     = ReadValue(reader, "product_id",     0L),
                SKU           = ReadValue(reader, "sku",           string.Empty),
                FrameSize     = ReadValue(reader, "frame_size",     string.Empty),
                Color         = ReadValue(reader, "color",         string.Empty),
                WheelSize     = ReadValue(reader, "wheel_size",     string.Empty),
                Price         = ReadValue(reader, "price",         string.Empty),
                StockQuantity = ReadValue(reader, "stock_quantity", 0),
                IsActive      = ReadValue(reader, "is_active",       false),
            };
        }

        public async Task<IEnumerable<ProductVariant>> GetAllAsync()
        {
            string sql = BaseSelect + " ORDER BY v.product_id, v.variant_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<ProductVariant?> GetByIdAsync(long id)
        {
            string sql = BaseSelect + " WHERE v.variant_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task<List<ProductVariant>> GetByProductIdAsync(long productId)
        {
            string sql = BaseSelect + " WHERE v.product_id = @productId ORDER BY v.variant_id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("productId", productId) });
            return items.ToList();
        }

        public async Task<List<ProductVariant>> GetByVariantIdAsync(long variantId)
        {
            return await GetByProductIdAsync(variantId);
        }

        public async Task AddAsync(ProductVariant entity)
        {
            string sql = @"
                INSERT INTO product_variants (product_id, sku, frame_size, color, wheel_size, price, stock_quantity, is_active)
                VALUES (@product_id, @sku, @frame_size, @color, @wheel_size, @price, @stock_quantity, @is_active)
                RETURNING variant_id";

            long? newId = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("product_id",     entity.ProductId),
                CreateParameter("sku",            entity.SKU),
                CreateParameter("frame_size",     entity.FrameSize),
                CreateParameter("color",          entity.Color),
                CreateParameter("wheel_size",     entity.WheelSize),
                CreateParameter("price",          entity.Price),
                CreateParameter("stock_quantity", entity.StockQuantity),
                CreateParameter("is_active",      entity.IsActive),
            });

            entity.VariantId = newId.GetValueOrDefault();
        }

        public async Task UpdateAsync(ProductVariant entity)
        {
            string sql = @"
                UPDATE product_variants
                SET product_id     = @product_id,
                    sku            = @sku,
                    frame_size     = @frame_size,
                    color          = @color,
                    wheel_size     = @wheel_size,
                    price          = @price,
                    stock_quantity = @stock_quantity,
                    is_active      = @is_active
                WHERE variant_id = @variant_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("product_id",     entity.ProductId),
                CreateParameter("sku",            entity.SKU),
                CreateParameter("frame_size",     entity.FrameSize),
                CreateParameter("color",          entity.Color),
                CreateParameter("wheel_size",     entity.WheelSize),
                CreateParameter("price",          entity.Price),
                CreateParameter("stock_quantity", entity.StockQuantity),
                CreateParameter("is_active",      entity.IsActive),
                CreateParameter("variant_id",     entity.VariantId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM product_variants WHERE variant_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> ExistsAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM product_variants WHERE variant_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<ProductVariant>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null)
        {
            string sql;
            string countSql;
            var queryParams = new List<DbParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search.Trim()}%";
                sql = BaseSelect + @"
                    WHERE v.sku LIKE @search OR v.frame_size LIKE @search OR v.color LIKE @search
                    ORDER BY v.variant_id
                    LIMIT @size OFFSET @offset";
                countSql = @"
                    SELECT COUNT(*) FROM product_variants v
                    WHERE v.sku LIKE @search OR v.frame_size LIKE @search OR v.color LIKE @search";

                queryParams.Add(CreateParameter("search", pattern));
            }
            else
            {
                sql = BaseSelect + @"
                    ORDER BY v.variant_id
                    LIMIT @size OFFSET @offset";
                countSql = "SELECT COUNT(*) FROM product_variants";
            }

            queryParams.Add(CreateParameter("size", pageSize));
            queryParams.Add(CreateParameter("offset", (pageNumber - 1) * pageSize));

            var items = await ExecuteReaderToListAsync(sql, MapReader, queryParams.ToArray());

            DbParameter[]? totalParams = !string.IsNullOrWhiteSpace(search)
                ? new[] { CreateParameter("search", $"%{search.Trim()}%") }
                : null;

            int total = await ExecuteScalarAsync<int>(countSql, totalParams);

            return new PaginationModel<ProductVariant>
            {
                Items       = items.ToList(),
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}
