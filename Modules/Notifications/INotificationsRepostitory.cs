using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Notifications
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetAllAsync();
        Task<Notification?> GetByIdAsync(long id);
        Task<List<Notification>> GetByUserIdAsync(long userId);
        Task AddAsync(Notification entity);
        Task UpdateAsync(Notification entity);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<PaginationModel<Notification>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
    }
}
