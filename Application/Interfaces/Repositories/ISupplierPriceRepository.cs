using Application.Interfaces.Repositories;
using Database.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ISupplierPriceRepository : IGenericRepository<SupplierPrice>
    {
        Task<IReadOnlyList<SupplierPrice>> GetBySupplierAsync(int supplierId);
        Task<IReadOnlyList<SupplierPrice>> GetByProductAsync(int productId);
        Task<SupplierPrice?> GetBySupplierAndProductAsync(int supplierId, int productId);
        Task<IReadOnlyList<SupplierPrice>> GetActiveAsync();
    }
} 