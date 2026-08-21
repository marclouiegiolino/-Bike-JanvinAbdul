using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.OrderItems
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetAllAsync();
        Task<OrderItem?> GetByIdAsync(long id);
        Task<List<OrderItem>> GetByOrderIdAsync(long orderId);
        Task<List<OrderItem>> GetByVariantIdAsync(long variantId);
        Task AddAsync(OrderItem entity);
        Task UpdateAsync(OrderItem entity);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<PaginationModel<OrderItem>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
    }
}
