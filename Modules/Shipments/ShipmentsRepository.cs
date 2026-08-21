using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Shipments
{
    public class ShipmentsRepository : BaseRepository, IShipmentsRepository
    {
        public ShipmentsRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT
                s.shipment_id,
                s.order_id,
                s.carrier_name,
                s.tracking_number,
                s.status,
                s.shipped_at,
                s.delivered_at
            FROM shipments s
            LEFT JOIN orders o ON o.order_id = s.order_id
        ";

        private Shipment MapReader(DbDataReader reader)
        {
            return new Shipment
            {
                ShipmentId   = ReadValue(reader, "shipment_id",   0L),
                OrderId  = ReadValue(reader, "order_id",  0L),
                CarrierName       = ReadValue(reader, "carrier_name",        string.Empty),
                TrackingNumber = ReadValue(reader, "tracking_number", string.Empty),
                Status      = ReadValue(reader, "status",       "active"),
                ShippedAt   = ReadValue(reader, "shipped_at",   string.Empty),
                DeliveredAt   = ReadValue(reader, "delivered_at",   string.Empty),
            };
        }

        public async Task<IEnumerable<Shipment>> GetAllAsync()
        {
            string sql = BaseSelect + " ORDER BY s.shipment_id, s.order_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Shipment?> GetByIdAsync(long id)
        {
            string sql = BaseSelect + " WHERE s.shipment_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task<List<Shipment>> GetByOrderIdAsync(long orderId)
        {
            string sql = BaseSelect + " WHERE s.order_id = @orderId ORDER BY s.shipment_id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("orderId", orderId) });
            return items.ToList();
        }

        public async Task AddAsync(Shipment entity)
        {
            string sql = @"
                INSERT INTO shipments (order_id, carrier_name, tracking_number, status, shipped_at, delivered_at)
                VALUES (@order_id, @carrier_name, @tracking_number, @status, @shipped_at, @delivered_at)
                RETURNING shipment_id";

            string status = string.IsNullOrWhiteSpace(entity.Status) ? "active" : entity.Status;

            long? newId = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("order_id",  entity.OrderId),
                CreateParameter("carrier_name",        entity.CarrierName),
                CreateParameter("tracking_number", entity.TrackingNumber),
                CreateParameter("status",       status),
                CreateParameter("shipped_at",   entity.ShippedAt),
                CreateParameter("delivered_at",   entity.DeliveredAt)
            });

            entity.ShipmentId = newId.GetValueOrDefault();
            entity.Status = status;
        }

        public async Task UpdateAsync(Shipment entity)
        {
            string sql = @"
                UPDATE shipments
                SET order_id  = @order_id,
                    carrier_name        = @carrier_name,
                    tracking_number = @tracking_number,
                    status       = @status,
                    shipped_at   = @shipped_at,
                    delivered_at   = @delivered_at
                WHERE shipment_id = @shipment_id";

            string status = string.IsNullOrWhiteSpace(entity.Status) ? "active" : entity.Status;

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("order_id",  entity.OrderId),
                CreateParameter("carrier_name",        entity.CarrierName),
                CreateParameter("tracking_number", entity.TrackingNumber),
                CreateParameter("status",       status),
                CreateParameter("shipped_at",   entity.ShippedAt),
                CreateParameter("delivered_at",   entity.DeliveredAt)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM shipments WHERE shipment_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> ExistsAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM shipments WHERE shipment_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<Shipment>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null)
        {
            string sql;
            string countSql;
            var queryParams = new List<DbParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search.Trim()}%";
                sql = BaseSelect + @"
                    WHERE s.order_id LIKE @search OR s.carrier_name LIKE @search OR s.tracking_number LIKE @search
                    ORDER BY s.shipment_id
                    LIMIT @size OFFSET @offset";
                countSql = @"
                    SELECT COUNT(*) FROM shipments s
                    WHERE s.order_id LIKE @search OR s.carrier_name LIKE @search OR s.tracking_number LIKE @search";

                queryParams.Add(CreateParameter("search", pattern));
            }
            else
            {
                sql = BaseSelect + @"
                    ORDER BY s.shipment_id
                    LIMIT @size OFFSET @offset";
                countSql = "SELECT COUNT(*) FROM shipments";
            }

            queryParams.Add(CreateParameter("size", pageSize));
            queryParams.Add(CreateParameter("offset", (pageNumber - 1) * pageSize));

            var items = await ExecuteReaderToListAsync(sql, MapReader, queryParams.ToArray());

            DbParameter[]? totalParams = !string.IsNullOrWhiteSpace(search)
                ? new[] { CreateParameter("search", $"%{search.Trim()}%") }
                : null;

            int total = await ExecuteScalarAsync<int>(countSql, totalParams);

            return new PaginationModel<Shipment>
            {
                Items       = items.ToList(),
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}
