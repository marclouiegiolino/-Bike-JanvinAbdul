using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Coupons
{
    public class CouponRepository : BaseRepository, ICouponRepository
    {
        public CouponRepository(MyCon dbConnection) : base(dbConnection) { }

        private Coupon MapReader(DbDataReader reader)
        {
            return new Coupon
            {
                CouponId     = ReadValue(reader, "coupon_id",     0L),
                Code         = ReadValue(reader, "code",         string.Empty),
                DiscountType = ReadValue(reader, "discount_type", string.Empty),
                DiscountValue= ReadValue(reader, "discount_value",string.Empty),
                ValidFrom    = ReadValue(reader, "valid_from",    string.Empty),
                ValidUntil   = ReadValue(reader, "valid_until",   string.Empty),
                IsActive     = ReadValue(reader, "is_active",     false)
            };
        }

        public async Task<IEnumerable<Coupon>> GetAllAsync()
        {
            string sql = "SELECT coupon_id, code, discount_type, discount_value, valid_from, valid_until, is_active FROM coupons ORDER BY coupon_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Coupon?> GetByIdAsync(long id)
        {
            string sql = "SELECT coupon_id, code, discount_type, discount_value, valid_from, valid_until, is_active FROM coupons WHERE coupon_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task AddAsync(Coupon entity)
        {
            string sql = @"INSERT INTO coupons (code, discount_type, discount_value, valid_from, valid_until, is_active) 
                          VALUES (@code, @discount_type, @discount_value, @valid_from, @valid_until, @is_active)
                          RETURNING coupon_id";

            long? result = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("code", entity.Code),
                CreateParameter("discount_type", entity.DiscountType),
                CreateParameter("discount_value", entity.DiscountValue),
                CreateParameter("valid_from", entity.ValidFrom),
                CreateParameter("valid_until", entity.ValidUntil),
                CreateParameter("is_active", entity.IsActive)
            });
            entity.CouponId = result.GetValueOrDefault();
        }

        public async Task UpdateAsync(Coupon entity)
        {
            string sql = @"
                UPDATE coupons
                SET code = @code,
                    discount_type = @discount_type,
                    discount_value = @discount_value,
                    valid_from = @valid_from,
                    valid_until = @valid_until,
                    is_active = @is_active
                WHERE coupon_id = @coupon_id";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("code", entity.Code),
                CreateParameter("discount_type", entity.DiscountType),
                CreateParameter("discount_value", entity.DiscountValue),
                CreateParameter("valid_from", entity.ValidFrom),
                CreateParameter("valid_until", entity.ValidUntil),
                CreateParameter("is_active", entity.IsActive),
                CreateParameter("coupon_id", entity.CouponId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM coupons WHERE coupon_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> IsInUseAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM coupons WHERE coupon_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<Coupon>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            string sql = @"
                SELECT coupon_id, code, discount_type, discount_value, valid_from, valid_until, is_active
                FROM coupons
                ORDER BY coupon_id
                LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapReader, new[]
            {
                CreateParameter("size", pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize)
            });

            int total = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM coupons");

            return new PaginationModel<Coupon>
            {
                Items = items.ToList(),
                TotalCount = total,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };
        }
    }
}