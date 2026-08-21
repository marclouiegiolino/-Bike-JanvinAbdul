using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Categories
{
    public class CategoriesRepository : BaseRepository, ICategoriesRepository
    {
        public CategoriesRepository(MyCon dbConnection) : base(dbConnection) { }

        private Categories MapReader(DbDataReader reader)
        {
            return new Categories
            {
                CategoryId   = ReadValue(reader, "category_id",   0L),
                CategoryName = ReadValue(reader, "category_name", string.Empty)
            };
        }

        public async Task<IEnumerable<Categories>> GetAllAsync()
        {
            string sql = "SELECT category_id, category_name FROM categories ORDER BY category_name";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Categories?> GetByIdAsync(long id)
        {
            string sql = "SELECT category_id, category_name FROM categories WHERE category_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task AddAsync(Categories entity)
        {
            string sql = @"INSERT INTO categories (category_name) 
                          VALUES (@category_name)
                          RETURNING category_id";

            long? result = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("category_name", entity.CategoryName)
            });
            entity.CategoryId = result.GetValueOrDefault();
        }

        public async Task UpdateAsync(Categories entity)
        {
            string sql = @"
                UPDATE categories
                SET category_name = @category_name
                WHERE category_id = @category_id";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("category_name", entity.CategoryName),
                CreateParameter("category_id", entity.CategoryId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM categories WHERE category_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> IsInUseAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM categories WHERE category_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<Categories>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            string sql = @"
                SELECT category_id, category_name
                FROM categories
                ORDER BY category_name
                LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapReader, new[]
            {
                CreateParameter("size", pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize)
            });

            int total = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM categories");

            return new PaginationModel<Categories>
            {
                Items = items.ToList(),
                TotalCount = total,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };
        }
    }
}