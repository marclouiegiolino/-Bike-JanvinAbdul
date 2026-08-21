using System.Data.Common;
using Api.Main;

namespace Api.Modules.Orders
{
    public class OrderRepository : BaseRepository, IOrderRepository
    {
        public OrderRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT
                o.order_id,
                o.user_id,
                o.shipping_address,
                o.subtotal,
                o.discount_amount,
                o.shipping_fee,
                o.total_amount,
                o.status,
                o.placed_at
            FROM orders o
            LEFT JOIN users u ON u.user_id = o.user_id
        ";

        private Order MapReader(DbDataReader reader)
        {
            return new Order
            {
                OrderId         = ReadValue(reader, "order_id",         0L),
                UserId          = ReadValue(reader, "user_id",          0L),
                ShippingAddress = ReadValue(reader, "shipping_address", string.Empty),
                Subtotal        = ReadValue(reader, "subtotal",         "0"),
                DiscountAmount  = ReadValue(reader, "discount_amount",  "0"),
                ShippingFee     = ReadValue(reader, "shipping_fee",     "0"),
                TotalAmount     = ReadValue(reader, "total_amount",     "0"),
                Status          = ReadValue(reader, "status",           string.Empty),
                PlacedAt        = ReadValue(reader, "placed_at",        string.Empty)
            };
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            string sql = BaseSelect + " ORDER BY o.user_id, o.order_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Order?> GetByIdAsync(long id)
        {
            string sql = BaseSelect + " WHERE o.order_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(long userId)
        {
            string sql = BaseSelect + " WHERE o.user_id = @user_id";
            return await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("user_id", userId) });
        }

        public async Task AddAsync(Order entity)
        {
            string sql = @"
                INSERT INTO orders (user_id, shipping_address, subtotal, discount_amount, shipping_fee, total_amount, status, placed_at)
                VALUES (@user_id, @shipping_address, @subtotal, @discount_amount, @shipping_fee, @total_amount, @status, @placed_at)
                RETURNING order_id";

            long? newId = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("user_id",        entity.UserId),
                CreateParameter("shipping_address", entity.ShippingAddress),
                CreateParameter("subtotal",       entity.Subtotal),
                CreateParameter("discount_amount",  entity.DiscountAmount),
                CreateParameter("shipping_fee",   entity.ShippingFee),
                CreateParameter("total_amount",   entity.TotalAmount),
                CreateParameter("status",         entity.Status),
                CreateParameter("placed_at",      entity.PlacedAt),
            });

            entity.OrderId = newId.GetValueOrDefault();
        }

        public async Task UpdateAsync(Order entity)
        {
            string sql = @"
                UPDATE orders
                SET user_id        = @user_id,
                    shipping_address = @shipping_address,
                    subtotal       = @subtotal,
                    discount_amount  = @discount_amount,
                    shipping_fee   = @shipping_fee,
                    total_amount   = @total_amount,
                    status         = @status,
                    placed_at      = @placed_at
                WHERE order_id = @order_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("user_id",        entity.UserId),
                CreateParameter("shipping_address", entity.ShippingAddress),
                CreateParameter("subtotal",       entity.Subtotal),
                CreateParameter("discount_amount",  entity.DiscountAmount),
                CreateParameter("shipping_fee",   entity.ShippingFee),
                CreateParameter("total_amount",   entity.TotalAmount),
                CreateParameter("status",         entity.Status),
                CreateParameter("placed_at",      entity.PlacedAt),
                CreateParameter("order_id",       entity.OrderId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM orders WHERE order_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> ExistsAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM orders WHERE order_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<Order>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            string sql = BaseSelect + @"
                ORDER BY o.user_id, o.order_id
                LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapReader, new[]
            {
                CreateParameter("size",   pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize),
            });

            int total = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM orders");

            return new PaginationModel<Order>
            {
                Items       = items,
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}
