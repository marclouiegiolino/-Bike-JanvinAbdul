using System.Data.Common;
using Api.Main;

namespace Api.Modules.Payments
{
    public class PaymentsRepository : BaseRepository, IPaymentsRepository
    {
        public PaymentsRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT
                py.payment_id,
                o.order_id,
                py.payment_type,
                py.amount,
                py.status,
                py.paid_at
            FROM payments py
            LEFT JOIN orders o ON o.order_id = py.order_id
        ";

        private Payment MapReader(DbDataReader reader)
        {
            return new Payment
            {
                PaymentId       = ReadValue(reader, "payment_id",       0L),
                OrderId         = ReadValue(reader, "order_id",         0L),
                PaymentType     = ReadValue(reader, "payment_type",     string.Empty),
                Amount          = ReadValue(reader, "amount",           0m),
                Status          = ReadValue(reader, "status",           string.Empty),
                PaidAt          = ReadValue(reader, "paid_at",          string.Empty)
            };
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            string sql = BaseSelect + " ORDER BY py.payment_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Payment?> GetByIdAsync(long id)
        {
            string sql = BaseSelect + " WHERE py.payment_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task<IEnumerable<Payment>> GetByOrderIdAsync(long orderId)
        {
            string sql = BaseSelect + " WHERE o.order_id = @order_id";
            return await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("order_id", orderId) });
        }

        public async Task AddAsync(Payment entity)
        {
            string sql = @"
                INSERT INTO payments (order_id, payment_type, amount, status, paid_at)
                VALUES (@order_id, @payment_type, @amount, @status, @paid_at)
                RETURNING payment_id";

            long? newId = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("order_id",         entity.OrderId),
                CreateParameter("payment_type",     entity.PaymentType),
                CreateParameter("amount",           entity.Amount),
                CreateParameter("status",           entity.Status),
                CreateParameter("paid_at",          entity.PaidAt),
            });

            entity.PaymentId = newId.GetValueOrDefault();
        }

        public async Task UpdateAsync(Payment entity)
        {
            string sql = @"
                UPDATE payments
                SET order_id       = @order_id,
                    payment_type     = @payment_type,
                    amount         = @amount,
                    status         = @status,
                    paid_at      = @paid_at
                WHERE payment_id = @payment_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("order_id",         entity.OrderId),
                CreateParameter("payment_type",     entity.PaymentType),
                CreateParameter("amount",           entity.Amount),
                CreateParameter("status",           entity.Status),
                CreateParameter("paid_at",          entity.PaidAt),
                CreateParameter("payment_id",       entity.PaymentId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM payments WHERE payment_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> ExistsAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM payments WHERE payment_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<Payment>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            string sql = BaseSelect + @"
                ORDER BY py.payment_id
                LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapReader, new[]
            {
                CreateParameter("size",   pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize),
            });

            int total = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM payments");

            return new PaginationModel<Payment>
            {
                Items       = items,
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}
