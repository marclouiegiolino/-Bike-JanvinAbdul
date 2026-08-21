using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.OrderItems
{
    public class OrderItemRepository : BaseRepository, IOrderItemRepository
    {
        public OrderItemRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT
                oi.order_item_id,
                oi.order_id,
                oi.variant_id,
                oi.quantity,
                oi.unit_price,
                oi.subtotal
            FROM order_items oi
            LEFT JOIN orders o ON o.order_id = oi.order_id
            LEFT JOIN product_variants pv ON pv.variant_id = oi.variant_id
        ";

        private OrderItem MapReader(DbDataReader reader)
        {
            return new OrderItem
            {
                OrderItemId = ReadValue(reader, "order_item_id", 0L),
                OrderId     = ReadValue(reader, "order_id",      0L),
                VariantId   = ReadValue(reader, "variant_id",    0L),
                Quantity    = ReadValue(reader, "quantity",      0),
                UnitPrice   = ReadValue(reader, "unit_price",    string.Empty),
                Subtotal    = ReadValue(reader, "subtotal",      string.Empty)
            };
        }

        public async Task<IEnumerable<OrderItem>> GetAllAsync()
        {
            string sql = BaseSelect + " ORDER BY oi.order_id, oi.order_item_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<OrderItem?> GetByIdAsync(long id)
        {
            string sql = BaseSelect + " WHERE oi.order_item_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task<List<OrderItem>> GetByOrderIdAsync(long orderId)
        {
            string sql = BaseSelect + " WHERE oi.order_id = @orderId ORDER BY oi.order_item_id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("orderId", orderId) });
            return items.ToList();
        }

        public async Task<List<OrderItem>> GetByVariantIdAsync(long variantId)
        {
            string sql = BaseSelect + " WHERE oi.variant_id = @variantId ORDER BY oi.order_item_id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("variantId", variantId) });
            return items.ToList();
        }

        public async Task AddAsync(OrderItem entity)
        {
            string sql = @"
                INSERT INTO order_items (order_id, variant_id, quantity, unit_price, subtotal)
                VALUES (@order_id, @variant_id, @quantity, @unit_price, @subtotal)
                RETURNING order_item_id";

            long? newId = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("order_id",     entity.OrderId),
                CreateParameter("variant_id",   entity.VariantId),
                CreateParameter("quantity",     entity.Quantity),
                CreateParameter("unit_price",   entity.UnitPrice),
                CreateParameter("subtotal",     entity.Subtotal),
            });

            entity.OrderItemId = newId.GetValueOrDefault();
        }

        public async Task UpdateAsync(OrderItem entity)
        {
            string sql = @"
                UPDATE order_items
                SET order_id     = @order_id,
                    variant_id   = @variant_id,
                    quantity     = @quantity,
                    unit_price   = @unit_price,
                    subtotal     = @subtotal
                WHERE order_item_id = @order_item_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("order_id",       entity.OrderId),
                CreateParameter("variant_id",     entity.VariantId),
                CreateParameter("quantity",       entity.Quantity),
                CreateParameter("unit_price",     entity.UnitPrice),
                CreateParameter("subtotal",       entity.Subtotal),
                CreateParameter("order_item_id",  entity.OrderItemId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM order_items WHERE order_item_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> ExistsAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM order_items WHERE order_item_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<OrderItem>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null)
        {
            string sql;
            string countSql;
            var queryParams = new List<DbParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search.Trim()}%";
                sql = BaseSelect + @"
                    WHERE oi.unit_price LIKE @search OR oi.subtotal LIKE @search
                    ORDER BY oi.order_item_id
                    LIMIT @size OFFSET @offset";
                countSql = @"
                    SELECT COUNT(*) FROM order_items oi
                    WHERE oi.unit_price LIKE @search OR oi.subtotal LIKE @search";

                queryParams.Add(CreateParameter("search", pattern));
            }
            else
            {
                sql = BaseSelect + @"
                    ORDER BY oi.order_item_id
                    LIMIT @size OFFSET @offset";
                countSql = "SELECT COUNT(*) FROM order_items";
            }

            queryParams.Add(CreateParameter("size", pageSize));
            queryParams.Add(CreateParameter("offset", (pageNumber - 1) * pageSize));

            var items = await ExecuteReaderToListAsync(sql, MapReader, queryParams.ToArray());

            DbParameter[]? totalParams = !string.IsNullOrWhiteSpace(search)
                ? new[] { CreateParameter("search", $"%{search.Trim()}%") }
                : null;

            int total = await ExecuteScalarAsync<int>(countSql, totalParams);

            return new PaginationModel<OrderItem>
            {
                Items       = items.ToList(),
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}
