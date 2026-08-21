using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Reviews
{
    public class ReviewRepository : BaseRepository, IReviewRepository
    {
        public ReviewRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT
                r.review_id,
                r.product_id,
                r.user_id,
                r.rating,
                r.comment,
                r.created_at
            FROM reviews r
            LEFT JOIN products pr ON pr.product_id = r.product_id
            LEFT JOIN users u ON u.user_id = r.user_id
        ";

        private Review MapReader(DbDataReader reader)
        {
            return new Review
            {
                ReviewId      = ReadValue(reader, "review_id",      0L),
                ProductId     = ReadValue(reader, "product_id",     0L),
                UserId        = ReadValue(reader, "user_id",        0L),
                Rating        = ReadValue(reader, "rating",         0),
                Comment       = ReadValue(reader, "comment",        string.Empty),
                CreatedAt     = ReadValue(reader, "created_at",     string.Empty)
            };
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            string sql = BaseSelect + " ORDER BY r.review_id, r.product_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Review?> GetByIdAsync(long id)
        {
            string sql = BaseSelect + " WHERE r.review_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task<List<Review>> GetByProductIdAsync(long productId)
        {
            string sql = BaseSelect + " WHERE r.product_id = @productId ORDER BY r.review_id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("productId", productId) });
            return items.ToList();
        }

        public async Task<List<Review>> GetByUserIdAsync(long userId)
        {
            string sql = BaseSelect + " WHERE r.user_id = @userId ORDER BY r.review_id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("userId", userId) });
            return items.ToList();
        }

        public async Task AddAsync(Review entity)
        {
            string sql = @"
                INSERT INTO reviews (product_id, user_id, rating, comment, created_at)
                VALUES (@product_id, @user_id, @rating, @comment, @created_at)
                RETURNING review_id";

            long? newId = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("product_id",     entity.ProductId),
                CreateParameter("user_id",        entity.UserId),
                CreateParameter("rating",         entity.Rating),
                CreateParameter("comment",        entity.Comment),
                CreateParameter("created_at",     entity.CreatedAt),
            });

            entity.ReviewId = newId.GetValueOrDefault();
        }

        public async Task UpdateAsync(Review entity)
        {
            string sql = @"
                UPDATE reviews
                SET product_id     = @product_id,
                    user_id        = @user_id,
                    rating         = @rating,
                    comment        = @comment,
                    created_at     = @created_at
                WHERE review_id = @review_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("product_id",     entity.ProductId),
                CreateParameter("user_id",        entity.UserId),
                CreateParameter("rating",         entity.Rating),
                CreateParameter("comment",        entity.Comment),
                CreateParameter("created_at",     entity.CreatedAt),
                CreateParameter("review_id",     entity.ReviewId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM reviews WHERE review_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> ExistsAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM reviews WHERE review_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<Review>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null)
        {
            string sql;
            string countSql;
            var queryParams = new List<DbParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search.Trim()}%";
                sql = BaseSelect + @"
                    WHERE r.product_id LIKE @search OR r.user_id LIKE @search OR r.rating LIKE @search OR r.comment LIKE @search
                    ORDER BY r.review_id
                    LIMIT @size OFFSET @offset";
                countSql = @"
                    SELECT COUNT(*) FROM reviews r
                    WHERE r.product_id LIKE @search OR r.user_id LIKE @search OR r.rating LIKE @search OR r.comment LIKE @search";

                queryParams.Add(CreateParameter("search", pattern));
            }
            else
            {
                sql = BaseSelect + @"
                    ORDER BY r.review_id
                    LIMIT @size OFFSET @offset";
                countSql = "SELECT COUNT(*) FROM reviews";
            }

            queryParams.Add(CreateParameter("size", pageSize));
            queryParams.Add(CreateParameter("offset", (pageNumber - 1) * pageSize));

            var items = await ExecuteReaderToListAsync(sql, MapReader, queryParams.ToArray());

            DbParameter[]? totalParams = !string.IsNullOrWhiteSpace(search)
                ? new[] { CreateParameter("search", $"%{search.Trim()}%") }
                : null;

            int total = await ExecuteScalarAsync<int>(countSql, totalParams);

            return new PaginationModel<Review>
            {
                Items       = items.ToList(),
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}
