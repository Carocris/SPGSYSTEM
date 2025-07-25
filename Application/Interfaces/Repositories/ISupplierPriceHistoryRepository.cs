using System.Collections.Generic;
using System.Threading.Tasks;
using Application.ViewModels.SupplierPriceHistory;
using Database.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ISupplierPriceHistoryRepository : IGenericRepository<SupplierPriceHistory>
    {
        Task<List<SupplierPriceHistoryViewModel>> GetBySupplierAsync(int supplierId);
        Task<List<SupplierPriceHistoryViewModel>> GetByProductAsync(int productId);
        Task<List<SupplierPriceHistoryViewModel>> GetBySupplierAndProductAsync(int supplierId, int productId);
        Task<List<SupplierPriceHistoryViewModel>> GetRecentAsync(int count = 10);
    }
} 