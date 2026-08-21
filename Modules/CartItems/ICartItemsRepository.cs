using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.CartItems
{
    public interface ICartItemsRepository
    {
        Task<IEnumerable<CartItems>> GetAllAsync();
        Task<CartItems?> GetByIdAsync(long id);
        Task<List<CartItems>> GetByVariantIdAsync(long variantId);
        Task<List<CartItems>> GetByUserIdAsync(long userId);
        Task AddAsync(CartItems entity);
        Task UpdateAsync(CartItems entity);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<PaginationModel<CartItems>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
    }
}
