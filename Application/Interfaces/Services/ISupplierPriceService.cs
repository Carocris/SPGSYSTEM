using Application.ViewModels.SupplierPrice;
using Database.Entities;

namespace Application.Interfaces.Services
{
    public interface ISupplierPriceService : IGenericService<SupplierPrice>
    {
        Task<IReadOnlyList<SupplierPrice>> GetBySupplierAsync(int supplierId);
        Task<IReadOnlyList<SupplierPrice>> GetByProductAsync(int productId);
        Task<SupplierPrice?> GetBySupplierAndProductAsync(int supplierId, int productId);
        Task<IReadOnlyList<SupplierPrice>> GetActiveAsync();
    }
} 