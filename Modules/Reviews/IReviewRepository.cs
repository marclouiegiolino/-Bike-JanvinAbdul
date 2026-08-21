using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Reviews
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<Review?> GetByIdAsync(long id);
        Task<List<Review>> GetByProductIdAsync(long productId);
        Task<List<Review>> GetByUserIdAsync(long userId);
        Task AddAsync(Review entity);
        Task UpdateAsync(Review entity);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<PaginationModel<Review>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
    }
}
