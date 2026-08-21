using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Modules.Shipments
{
    public interface IShipmentsRepository
    {
        Task<IEnumerable<Shipment>> GetAllAsync();
        Task<Shipment?> GetByIdAsync(long id);
        Task<List<Shipment>> GetByOrderIdAsync(long orderId);
        Task AddAsync(Shipment entity);
        Task UpdateAsync(Shipment entity);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<PaginationModel<Shipment>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
    }
}
