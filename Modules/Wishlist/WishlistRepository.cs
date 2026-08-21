using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Wishlist
{
    public class WishlistRepository : BaseRepository, IWishlistRepository
    {
        public WishlistRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT
                w.wishlist_id,
                w.user_id,
                w.variant_id,
                w.added_at
            FROM wishlists w
            LEFT JOIN product_variants v ON v.variant_id = w.variant_id
            LEFT JOIN users u ON u.user_id = w.user_id
        ";

        private Wishlist MapReader(DbDataReader reader)
        {
            return new Wishlist
            {
                WishlistId      = ReadValue(reader, "wishlist_id",      0L),
                UserId          = ReadValue(reader, "user_id",        0L),
                VariantId       = ReadValue(reader, "variant_id",       0L),
                AddedAt         = ReadValue(reader, "added_at",     string.Empty)
            };
        }

        public async Task<IEnumerable<Wishlist>> GetAllAsync()
        {
            string sql = BaseSelect + " ORDER BY w.wishlist_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Wishlist?> GetByIdAsync(long id)
        {
            string sql = BaseSelect + " WHERE w.wishlist_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task<List<Wishlist>> GetByVariantIdAsync(long variantId)
        {
            string sql = BaseSelect + " WHERE w.variant_id = @variantId ORDER BY w.wishlist_id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("variantId", variantId) });
            return items.ToList();
        }

        public async Task<List<Wishlist>> GetByUserIdAsync(long userId)
        {
            string sql = BaseSelect + " WHERE w.user_id = @userId ORDER BY w.wishlist_id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("userId", userId) });
            return items.ToList();
        }

        public async Task AddAsync(Wishlist entity)
        {
            string sql = @"
                INSERT INTO wishlists (user_id, variant_id, added_at)
                VALUES (@user_id, @variant_id, @added_at)
                RETURNING wishlist_id";

            long? newId = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("user_id",        entity.UserId),
                CreateParameter("variant_id",     entity.VariantId),
                CreateParameter("added_at",       entity.AddedAt),
            });

            entity.WishlistId = newId.GetValueOrDefault();
        }

        public async Task UpdateAsync(Wishlist entity)
        {
            string sql = @"
                UPDATE wishlists
                SET user_id        = @user_id,
                    variant_id     = @variant_id,
                    added_at       = @added_at
                WHERE wishlist_id = @wishlist_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("user_id",        entity.UserId),
                CreateParameter("variant_id",     entity.VariantId),
                CreateParameter("added_at",       entity.AddedAt),
                CreateParameter("wishlist_id",    entity.WishlistId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM wishlists WHERE wishlist_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> ExistsAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM wishlists WHERE wishlist_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<Wishlist>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null)
        {
            string sql;
            string countSql;
            var queryParams = new List<DbParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search.Trim()}%";
                sql = BaseSelect + @"
                    WHERE w.user_id LIKE @search OR w.variant_id LIKE @search OR w.added_at LIKE @search
                    ORDER BY w.wishlist_id
                    LIMIT @size OFFSET @offset";
                countSql = @"
                    SELECT COUNT(*) FROM wishlists w
                    WHERE w.user_id LIKE @search OR w.variant_id LIKE @search OR w.added_at LIKE @search";

                queryParams.Add(CreateParameter("search", pattern));
            }
            else
            {
                sql = BaseSelect + @"
                    ORDER BY w.wishlist_id
                    LIMIT @size OFFSET @offset";
                countSql = "SELECT COUNT(*) FROM wishlists";
            }

            queryParams.Add(CreateParameter("size", pageSize));
            queryParams.Add(CreateParameter("offset", (pageNumber - 1) * pageSize));

            var items = await ExecuteReaderToListAsync(sql, MapReader, queryParams.ToArray());

            DbParameter[]? totalParams = !string.IsNullOrWhiteSpace(search)
                ? new[] { CreateParameter("search", $"%{search.Trim()}%") }
                : null;

            int total = await ExecuteScalarAsync<int>(countSql, totalParams);

            return new PaginationModel<Wishlist>
            {
                Items       = items.ToList(),
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}
