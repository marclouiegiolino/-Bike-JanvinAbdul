using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;
using Api.Main;

namespace Api.Modules.Users
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(MyCon dbConnection) : base(dbConnection) { }


        private User MapReaderToUser(DbDataReader reader)
        {
            try
            {
                return new User()
                {
                    UserId = ReadValue<int>(reader, "user_id", 0),
                    RoleId = ReadValue<int>(reader, "role_id", 0 ),
                    FirstName = ReadValue<string>(reader, "first_name", string.Empty),
                    LastName = ReadValue<string>(reader, "last_name", string.Empty),
                    PhoneNumber = ReadValue<string>(reader, "phone_number", string.Empty),
                    UserName = ReadValue<string>(reader, "username", string.Empty),
                    PasswordHash = ReadValue<string>(reader, "password_hash", string.Empty),
                    IsActive = ReadValue<string>(reader, "is_active", string.Empty),
                    CreatedAt = ReadValue<string>(reader, "created_at", string.Empty)
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Failed to map database row to User. Check schema/type alignment for generated columns.",
                    ex);
            }
        }

        /// <summary>
        /// Retrieves all entities from the database
        /// </summary>
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await ExecuteReaderToListAsync("SELECT * FROM \"users\"", MapReaderToUser);
        }

        /// <summary>
        /// Retrieves an entity by UserId
        /// </summary>
        public async Task<User?> GetByIdAsync(int userId)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"users\" WHERE \"user_id\" = @userId", 
                MapReaderToUser, 
                new[] { CreateParameter("userId", userId) });
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves an entity by UserName
        /// </summary>
        public async Task<User?> GetByUserNameAsync(string userName)
        {
            var results = await ExecuteReaderToListAsync(
                "SELECT * FROM \"users\" WHERE \"username\" = @userName",
                MapReaderToUser,
                new[] { CreateParameter("userName", userName) });
            return results.FirstOrDefault();
        }


        /// <summary>
        /// Adds a new entity to the database
        /// </summary>
        public async Task AddAsync(User entity)
        {
            var sql = "INSERT INTO \"users\" (\"role_id\", \"first_name\", \"last_name\", \"phone_number\", \"username\", \"password_hash\", \"is_active\", \"created_at\") VALUES (@roleId, @firstName, @lastName, @phoneNumber, @userName, @passwordHash, @isActive, @createdAt)";
            var dbParameters = new List<DbParameter>();
            dbParameters.Add(CreateParameter("roleId", entity.RoleId));
            dbParameters.Add(CreateParameter("firstName", entity.FirstName));
            dbParameters.Add(CreateParameter("lastName", entity.LastName));
            dbParameters.Add(CreateParameter("phoneNumber", entity.PhoneNumber));
            dbParameters.Add(CreateParameter("userName", entity.UserName));
            dbParameters.Add(CreateParameter("passwordHash", entity.PasswordHash));
            dbParameters.Add(CreateParameter("isActive", string.IsNullOrEmpty(entity.IsActive) ? "1" : entity.IsActive));
            dbParameters.Add(CreateParameter("createdAt", string.IsNullOrEmpty(entity.CreatedAt) ? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") : entity.CreatedAt));
            await ExecuteNonQueryAsync(sql, dbParameters.ToArray());
        }

        /// <summary>
        /// Updates an existing entity in the database
        /// </summary>
        public async Task UpdateAsync(User entity)
        {
            var sql = "UPDATE \"users\" SET \"role_id\" = @roleId, \"first_name\" = @firstName, \"last_name\" = @lastName, \"phone_number\" = @phoneNumber, \"username\" = @userName, \"password_hash\" = @passwordHash, \"is_active\" = @isActive, \"created_at\" = @createdAt WHERE \"user_id\" = @userId";
            var dbParameters = new List<DbParameter>();
            dbParameters.Add(CreateParameter("userId", entity.UserId));
            dbParameters.Add(CreateParameter("roleId", entity.RoleId));
            dbParameters.Add(CreateParameter("firstName", entity.FirstName));
            dbParameters.Add(CreateParameter("lastName", entity.LastName));
            dbParameters.Add(CreateParameter("phoneNumber", entity.PhoneNumber));
            dbParameters.Add(CreateParameter("userName", entity.UserName));
            dbParameters.Add(CreateParameter("passwordHash", entity.PasswordHash));
            dbParameters.Add(CreateParameter("isActive", entity.IsActive));
            dbParameters.Add(CreateParameter("createdAt", entity.CreatedAt));
            await ExecuteNonQueryAsync(sql, dbParameters.ToArray());
        }

        /// <summary>
        /// Deletes an entity from the database by its UserId
        /// </summary>
        public async Task DeleteAsync(int userId)
        {
            await ExecuteNonQueryAsync("DELETE FROM \"users\" WHERE \"user_id\" = @userId", new[] { CreateParameter("userId", userId) });
        }

        /// <summary>
        /// Deletes all entities from the database
        /// </summary>
        public async Task DeleteAllAsync()
        {
            await ExecuteNonQueryAsync("DELETE FROM \"users\"");
        }

        private static int? GetFilterInt(IReadOnlyDictionary<string, object?> filters, string key)
        {
            if (!filters.TryGetValue(key, out object? rawValue) || rawValue == null)
            {
                return null;
            }

            if (rawValue is int intValue)
            {
                return intValue;
            }

            string text = Convert.ToString(rawValue, CultureInfo.InvariantCulture) ?? string.Empty;
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                ? parsed
                : null;
        }

        private static string? GetFilterText(IReadOnlyDictionary<string, object?> filters, string key)
        {
            if (!filters.TryGetValue(key, out object? rawValue) || rawValue == null)
            {
                return null;
            }

            string text = (Convert.ToString(rawValue, CultureInfo.InvariantCulture) ?? string.Empty).Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private static bool TryParseTimeToken(string value, out TimeSpan parsedTime, out bool hasSeconds)
        {
            parsedTime = default;
            hasSeconds = false;

            string normalized = value.Trim();
            if (TimeSpan.TryParseExact(normalized, "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out parsedTime))
            {
                hasSeconds = true;
                return true;
            }

            return TimeSpan.TryParseExact(normalized, "hh\\:mm", CultureInfo.InvariantCulture, out parsedTime);
        }


        public async Task<IEnumerable<User>> GetFilteredExactAsync(IReadOnlyDictionary<string, object?> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return await GetAllAsync();
            }

            var sqlFilters = new List<string>();
            var dbParameters = new List<DbParameter>();
            if (filters.TryGetValue("userId", out object? userIdFilter) && userIdFilter != null)
            {
                sqlFilters.Add("\"user_id\" = @userId");
                dbParameters.Add(CreateParameter("userId", userIdFilter));
            }

            IEnumerable<User> queryable = sqlFilters.Count == 0
                ? await GetAllAsync()
                : await ExecuteReaderToListAsync(
                    $"SELECT * FROM \"users\" WHERE {string.Join(" AND ", sqlFilters)}",
                    MapReaderToUser,
                    dbParameters.ToArray());
            return queryable.ToList();
        }

        public async Task<IEnumerable<User>> GetFilteredLikeAsync(IReadOnlyDictionary<string, object?> filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return await GetAllAsync();
            }

            var sqlFilters = new List<string>();
            var dbParameters = new List<DbParameter>();
            string? userIdFilter = GetFilterText(filters, "userId");
            if (!string.IsNullOrWhiteSpace(userIdFilter))
            {
                string userIdPattern = "%" + userIdFilter + "%";
                sqlFilters.Add("CAST(\"user_id\" AS TEXT) LIKE @userIdPattern");
                dbParameters.Add(CreateParameter("userIdPattern", userIdPattern));
            }

            IEnumerable<User> queryable = sqlFilters.Count == 0
                ? await GetAllAsync()
                : await ExecuteReaderToListAsync(
                    $"SELECT * FROM \"users\" WHERE {string.Join(" AND ", sqlFilters)}",
                    MapReaderToUser,
                    dbParameters.ToArray());
            return queryable.ToList();
        }

        /// <summary>
        /// Searches for entities by query string
        /// </summary>
        public async Task<IEnumerable<User>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllAsync();
            }
            return await SearchAsyncAll(query);
        }

        /// <summary>
        /// Searches for entities by all column
        /// </summary>
        public async Task<IEnumerable<User>> SearchAsyncAll(string query)
        {
            var filters = new List<string>();
            filters.Add("CAST(\"user_id\" AS TEXT) LIKE @query");
            if (filters.Count == 0)
            {
                return await GetAllAsync();
            }
            string sql = $"SELECT * FROM \"users\" WHERE {string.Join(" OR ", filters)}";
            return await ExecuteReaderToListAsync(sql, MapReaderToUser, new[] { CreateParameter("query", "%" + query + "%") });
        }

        /// <summary>
        /// Retrieves a paginated list of entities
        /// </summary>
        public async Task<PaginationModel<User>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            if (pageSize < 0)
            {
                pageSize = 0;
            }

            if (pageSize <= 0)
            {
                var allItems = await ExecuteReaderToListAsync("SELECT * FROM \"users\"", MapReaderToUser);
                var totalRecords = allItems.Count;
                return new PaginationModel<User>()
                {
                    Items = allItems,
                    TotalCount = totalRecords,
                    PageSize = totalRecords,
                    CurrentPage = 1
                };
            }

            string sql = "SELECT * FROM \"users\" ORDER BY \"user_id\" LIMIT @pageSize OFFSET @offset";
            var items = await ExecuteReaderToListAsync(sql, MapReaderToUser, new[]
            {
                CreateParameter("offset", (pageNumber - 1) * pageSize),
                CreateParameter("pageSize", pageSize)
            });

            var totalRecordsCount = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"users\"");
            return new PaginationModel<User>()
            {
                Items = items,
                TotalCount = totalRecordsCount,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };
        }

        /// <summary>
        /// Uploads data from a list to the database.
        /// </summary>
        public async Task BulkUploadAsync(List<User> dataList)
        {
            if (dataList == null || dataList.Count == 0) return;

            foreach (var item in dataList)
            {
                await AddAsync(item);
            }
        }
    }
}