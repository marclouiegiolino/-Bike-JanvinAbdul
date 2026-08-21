using Api.Main;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Modules.Coupons
{
	public interface ICouponRepository
	{
		Task<IEnumerable<Coupon>> GetAllAsync();
		Task<Coupon?> GetByIdAsync(long id);
		Task AddAsync(Coupon entity);
		Task UpdateAsync(Coupon entity);
		Task DeleteAsync(long id);
		Task<bool> IsInUseAsync(long id);
		Task<PaginationModel<Coupon>> GetPaginatedAsync(int pageNumber, int pageSize);
	}
}
