using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.CartItems
{
    public class CartItemsRepository : BaseRepository, ICartItemsRepository
    {
        public CartItemsRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT
                ci.cart_item_id,
                ci.user_id,
                ci.variant_id,
                ci.quantity,
                ci.added_at
            FROM cart_items ci
            LEFT JOIN product_variants pv ON pv.variant_id = ci.variant_id
            LEFT JOIN users u ON u.user_id = ci.user_id
        ";

        private CartItems MapReader(DbDataReader reader)
        {
            return new CartItems
            {
                CartItemId      = ReadValue(reader, "cart_item_id",      0L),
                UserId          = ReadValue(reader, "user_id",          0L),
                VariantId       = ReadValue(reader, "variant_id",       0L),
                Quantity        = ReadValue(reader, "quantity",         0),
                AddedAt         = ReadValue(reader, "added_at",         string.Empty),
            };
        }

        public async Task<IEnumerable<CartItems>> GetAllAsync()
        {
            string sql = BaseSelect + " ORDER BY ci.cart_item_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<CartItems?> GetByIdAsync(long id)
        {
            string sql = BaseSelect + " WHERE ci.cart_item_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task<List<CartItems>> GetByUserIdAsync(long userId)
        {
            string sql = BaseSelect + " WHERE ci.user_id = @userId ORDER BY ci.cart_item_id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("userId", userId) });
            return items.ToList();
        }

        public async Task<List<CartItems>> GetByVariantIdAsync(long variantId)
        {
            string sql = BaseSelect + " WHERE ci.variant_id = @variantId ORDER BY ci.cart_item_id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("variantId", variantId) });
            return items.ToList();
        }

        public async Task AddAsync(CartItems entity)
        {
            string sql = @"
                INSERT INTO cart_items (variant_id, user_id, quantity, added_at)
                VALUES (@variant_id, @user_id, @quantity, @added_at)
                RETURNING cart_item_id";

            long? newId = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("variant_id",     entity.VariantId),
                CreateParameter("user_id",        entity.UserId),
                CreateParameter("quantity",       entity.Quantity),
                CreateParameter("added_at",       entity.AddedAt),
            });

            entity.CartItemId = newId.GetValueOrDefault();
        }

        public async Task UpdateAsync(CartItems entity)
        {
            string sql = @"
                UPDATE cart_items
                SET variant_id     = @variant_id,
                    user_id        = @user_id,
                    quantity       = @quantity,
                    added_at       = @added_at
                WHERE cart_item_id = @cart_item_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("variant_id",     entity.VariantId),
                CreateParameter("user_id",        entity.UserId),
                CreateParameter("quantity",       entity.Quantity),
                CreateParameter("added_at",       entity.AddedAt),
                CreateParameter("cart_item_id",   entity.CartItemId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM cart_items WHERE cart_item_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> ExistsAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM cart_items WHERE cart_item_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<CartItems>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null)
        {
            string sql;
            string countSql;
            var queryParams = new List<DbParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search.Trim()}%";
                sql = BaseSelect + @"
                    WHERE ci.variant_id LIKE @search OR ci.user_id LIKE @search OR ci.quantity LIKE @search OR ci.added_at LIKE @search
                    ORDER BY ci.cart_item_id
                    LIMIT @size OFFSET @offset";
                countSql = @"
                    SELECT COUNT(*) FROM cart_items ci
                    WHERE ci.variant_id LIKE @search OR ci.user_id LIKE @search OR ci.quantity LIKE @search OR ci.added_at LIKE @search";

                queryParams.Add(CreateParameter("search", pattern));
            }
            else
            {
                sql = BaseSelect + @"
                    ORDER BY ci.cart_item_id
                    LIMIT @size OFFSET @offset";
                countSql = "SELECT COUNT(*) FROM cart_items";
            }

            queryParams.Add(CreateParameter("size", pageSize));
            queryParams.Add(CreateParameter("offset", (pageNumber - 1) * pageSize));

            var items = await ExecuteReaderToListAsync(sql, MapReader, queryParams.ToArray());

            DbParameter[]? totalParams = !string.IsNullOrWhiteSpace(search)
                ? new[] { CreateParameter("search", $"%{search.Trim()}%") }
                : null;

            int total = await ExecuteScalarAsync<int>(countSql, totalParams);

            return new PaginationModel<CartItems>
            {
                Items       = items.ToList(),
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}
