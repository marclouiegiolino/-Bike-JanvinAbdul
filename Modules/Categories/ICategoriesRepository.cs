using Api.Main;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Modules.Categories
{
	public interface ICategoriesRepository
	{
		Task<IEnumerable<Categories>> GetAllAsync();
		Task<Categories?> GetByIdAsync(long id);
		Task AddAsync(Categories entity);
		Task UpdateAsync(Categories entity);
		Task DeleteAsync(long id);
		Task<bool> IsInUseAsync(long id);
		Task<PaginationModel<Categories>> GetPaginatedAsync(int pageNumber, int pageSize);
	}
}
