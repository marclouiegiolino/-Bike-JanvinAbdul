using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Notifications
{
    public class NotificationRepository : BaseRepository, INotificationRepository
    {
        public NotificationRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT
                n.notification_id,
                n.user_id,
                n.type,
                n.message,
                n.is_read,
                n.created_at
            FROM notifications n
            LEFT JOIN users u ON u.user_id = n.user_id
        ";

        private Notification MapReader(DbDataReader reader)
        {
            return new Notification
            {
                NotificationId   = ReadValue(reader, "notification_id",   0L),
                UserId  = ReadValue(reader, "user_id",  0L),
                Type       = ReadValue(reader, "type",        string.Empty),
                Message = ReadValue(reader, "message", string.Empty),
                IsRead = ReadValue(reader, "is_read",  false),
                CreatedAt   = ReadValue(reader, "created_at",   string.Empty),
            };
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            string sql = BaseSelect + " ORDER BY n.notification_id, n.user_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Notification?> GetByIdAsync(long id)
        {
            string sql = BaseSelect + " WHERE n.notification_id = @id";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("id", id) });
            return items.FirstOrDefault();
        }

        public async Task<List<Notification>> GetByUserIdAsync(long userId)
        {
            string sql = BaseSelect + " WHERE n.user_id = @userId ORDER BY n.created_at DESC";
            var items = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("userId", userId) });
            return items.ToList();
        }

        public async Task AddAsync(Notification entity)
        {
            string sql = @"
                INSERT INTO notifications (user_id, type, message, is_read, created_at)
                VALUES (@user_id, @type, @message, @is_read, @created_at)
                RETURNING notification_id";

            bool isRead = entity.IsRead;
            string createdAt = string.IsNullOrWhiteSpace(entity.CreatedAt)
                ? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
                : entity.CreatedAt;

            long? newId = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("user_id",  entity.UserId),
                CreateParameter("type",        entity.Type),
                CreateParameter("message", entity.Message),
                CreateParameter("is_read",  entity.IsRead),
                CreateParameter("created_at",   createdAt),
            });

            entity.NotificationId = newId.GetValueOrDefault();
            entity.IsRead = isRead;
            entity.CreatedAt = createdAt;
        }

        public async Task UpdateAsync(Notification entity)
        {
            string sql = @"
                UPDATE notifications
                SET user_id  = @user_id,
                    type        = @type,
                    message = @message,
                    is_read  = @is_read,
                    created_at   = @created_at
                WHERE notification_id = @notification_id";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("user_id",  entity.UserId),
                CreateParameter("type",        entity.Type),
                CreateParameter("message", entity.Message),
                CreateParameter("is_read",  entity.IsRead),
                CreateParameter("created_at",   entity.CreatedAt),
                CreateParameter("notification_id",   entity.NotificationId)
            });
        }

        public async Task DeleteAsync(long id)
        {
            string sql = "DELETE FROM notifications WHERE notification_id = @id";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("id", id) });
        }

        public async Task<bool> ExistsAsync(long id)
        {
            string sql = "SELECT COUNT(*) FROM notifications WHERE notification_id = @id";
            var count = await ExecuteScalarAsync<int>(sql, new[] { CreateParameter("id", id) });
            return count > 0;
        }

        public async Task<PaginationModel<Notification>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null)
        {
            string sql;
            string countSql;
            var queryParams = new List<DbParameter>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search.Trim()}%";
                sql = BaseSelect + @"
                    WHERE n.message LIKE @search OR n.type LIKE @search
                    ORDER BY n.notification_id
                    LIMIT @size OFFSET @offset";
                countSql = @"
                    SELECT COUNT(*) FROM notifications n
                    WHERE n.message LIKE @search OR n.type LIKE @search";

                queryParams.Add(CreateParameter("search", pattern));
            }
            else
            {
                sql = BaseSelect + @"
                    ORDER BY n.notification_id
                    LIMIT @size OFFSET @offset";
                countSql = "SELECT COUNT(*) FROM notifications";
            }

            queryParams.Add(CreateParameter("size", pageSize));
            queryParams.Add(CreateParameter("offset", (pageNumber - 1) * pageSize));

            var items = await ExecuteReaderToListAsync(sql, MapReader, queryParams.ToArray());

            DbParameter[]? totalParams = !string.IsNullOrWhiteSpace(search)
                ? new[] { CreateParameter("search", $"%{search.Trim()}%") }
                : null;

            int total = await ExecuteScalarAsync<int>(countSql, totalParams);

            return new PaginationModel<Notification>
            {
                Items       = items.ToList(),
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}
