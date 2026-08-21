using System.Data.Common;
using Api.Main;

namespace Api.Modules.Addresses
{
    public class AddressRepository : BaseRepository, IAddressRepository
    {
        public AddressRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT
                a.address_id,
                a.user_id,
                a.recipient_name,
                a.phone_number,
                a.city,
                a.province_state,
                a.postal_code,
                a.country,
                a.is_default
            FROM addresses a
            LEFT JOIN users u ON u.user_id = a.user_id
        ";

        private Address MapReader(DbDataReader reader)
        {
            return new Address
            {
                AddressId     = ReadValue(reader, "address_id",     0L),
                UserId        = ReadValue(reader, "user_id",        0L),
                RecipientName = ReadValue(reader, "recipient_name", string.Empty),
                PhoneNumber   = ReadValue(reader, "phone_number",   string.Empty),
                City          = ReadValue(reader, "city",           string.Empty),
                ProvinceState = ReadValue(reader, "province_state", string.Empty),
                PostalCode    = ReadValue(reader, "postal_code",    string.Empty),
                Country       = ReadValue(reader, "country",        string.Empty),
                IsDefault     = ReadValue(reader, "is_default",     false),
            };
        }

        public async Task<IEnumerable<Address>> GetAllAsync()
        {
            string sql = BaseSelect + " ORDER BY a.user_id, a.address_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Address?> GetByIdAsync(long id)
        {
            string sql = BaseSelect + " WHERE a.address_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task AddAsync(Address entity)
        {
            string sql = @"
                INSERT INTO addresses (user_id, recipient_name, phone_number, city, province_state, postal_code, country, is_default)
                VALUES (@user_id, @recipient_name, @phone_number, @city, @province_state, @postal_code, @country, @is_default)
                RETURNING address_id";

            long? newId = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("user_id",        entity.UserId),
                CreateParameter("recipient_name", entity.RecipientName),
                CreateParameter("phone_number",   entity.PhoneNumber),
                CreateParameter("city",           entity.City),
                CreateParameter("province_state", entity.ProvinceState),
                CreateParameter("postal_code",    entity.PostalCode),
                CreateParameter("country",        entity.Country),
                CreateParameter("is_default",     entity.IsDefault),
            });

            entity.AddressId = newId.GetValueOrDefault();
        }

        public async Task UpdateAsync(Address entity)
        {
            string sql = @"
                UPDATE addresses
                SET user_id        = @user_id,
                    recipient_name = @recipient_name,
                    phone_number   = @phone_number,
                    city           = @city,
                    province_state = @province_state,
                    postal_code    = @postal_code,
                    country        = @country,
                    is_default     = @is_default
                WHERE address_id = @address_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("user_id",        entity.UserId),
                CreateParameter("recipient_name", entity.RecipientName),
                CreateParameter("phone_number",   entity.PhoneNumber),
                CreateParameter("city",           entity.City),
                CreateParameter("province_state", entity.ProvinceState),
                CreateParameter("postal_code",    entity.PostalCode),
                CreateParameter("country",        entity.Country),
                CreateParameter("is_default",     entity.IsDefault),
                CreateParameter("address_id",     entity.AddressId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM addresses WHERE address_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> ExistsAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM addresses WHERE address_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<Address>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            string sql = BaseSelect + @"
                ORDER BY a.user_id, a.address_id
                LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapReader, new[]
            {
                CreateParameter("size",   pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize),
            });

            int total = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM addresses");

            return new PaginationModel<Address>
            {
                Items       = items,
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}
