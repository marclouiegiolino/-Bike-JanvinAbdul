using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Wishlist
{
    public interface IWishlistRepository
    {
        Task<IEnumerable<Wishlist>> GetAllAsync();
        Task<Wishlist?> GetByIdAsync(long id);
        Task<List<Wishlist>> GetByVariantIdAsync(long variantId);
        Task<List<Wishlist>> GetByUserIdAsync(long userId);
        Task AddAsync(Wishlist entity);
        Task UpdateAsync(Wishlist entity);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<PaginationModel<Wishlist>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
    }
}
