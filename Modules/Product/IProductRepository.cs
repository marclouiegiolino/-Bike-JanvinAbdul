using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Product
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(long id);
        Task<List<Product>> GetByCategoryIdAsync(long categoryId);
        Task AddAsync(Product entity);
        Task UpdateAsync(Product entity);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<PaginationModel<Product>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
    }
}
