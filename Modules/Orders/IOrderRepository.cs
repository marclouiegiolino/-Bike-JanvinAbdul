using Api.Main;

namespace Api.Modules.Orders
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order?> GetByIdAsync(long id);
        Task<IEnumerable<Order>> GetByUserIdAsync(long userId);
        Task<PaginationModel<Order>> GetPaginatedAsync(int pageNumber, int pageSize);
        Task AddAsync(Order entity);
        Task UpdateAsync(Order entity);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
    }
}
