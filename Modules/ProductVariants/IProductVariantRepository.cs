using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.ProductVariants
{
    public interface IProductVariantRepository
    {
        Task<IEnumerable<ProductVariant>> GetAllAsync();
        Task<ProductVariant?> GetByIdAsync(long id);
        Task<List<ProductVariant>> GetByProductIdAsync(long productId);
        Task<List<ProductVariant>> GetByVariantIdAsync(long variantId);
        Task AddAsync(ProductVariant entity);
        Task UpdateAsync(ProductVariant entity);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<PaginationModel<ProductVariant>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
    }
}
