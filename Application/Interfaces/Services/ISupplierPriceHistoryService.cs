using System.Collections.Generic;
using System.Threading.Tasks;
using Application.ViewModels.SupplierPriceHistory;
using Database.Entities;

namespace Application.Interfaces.Services
{
    public interface ISupplierPriceHistoryService : IGenericService<SupplierPriceHistory>
    {
        Task<List<SupplierPriceHistoryViewModel>> GetBySupplierAsync(int supplierId);
        Task<List<SupplierPriceHistoryViewModel>> GetByProductAsync(int productId);
        Task<List<SupplierPriceHistoryViewModel>> GetBySupplierAndProductAsync(int supplierId, int productId);
        Task<List<SupplierPriceHistoryViewModel>> GetRecentAsync(int count = 10);
        Task CreateHistoryRecordAsync(int supplierId, int productId, decimal oldPrice, decimal newPrice, string? changedBy = null, string? notes = null, string? changeReason = null);
    }
} 