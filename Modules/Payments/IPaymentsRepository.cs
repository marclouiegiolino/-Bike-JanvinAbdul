using Api.Main;

namespace Api.Modules.Payments
{
    public interface IPaymentsRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(long id);
        Task<IEnumerable<Payment>> GetByOrderIdAsync(long userId);
        Task<PaginationModel<Payment>> GetPaginatedAsync(int pageNumber, int pageSize);
        Task AddAsync(Payment entity);
        Task UpdateAsync(Payment entity);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
    }
}
