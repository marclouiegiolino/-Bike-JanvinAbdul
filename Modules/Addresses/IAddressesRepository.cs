using Api.Main;

namespace Api.Modules.Addresses
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetAllAsync();
        Task<Address?> GetByIdAsync(long id);
        Task AddAsync(Address entity);
        Task UpdateAsync(Address entity);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<PaginationModel<Address>> GetPaginatedAsync(int pageNumber, int pageSize);
    }
}
