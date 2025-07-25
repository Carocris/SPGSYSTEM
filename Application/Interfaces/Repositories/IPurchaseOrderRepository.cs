using System.Collections.Generic;
using System.Threading.Tasks;
using Application.ViewModels.PurchaseOrder;
using Database.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IPurchaseOrderRepository : IGenericRepository<PurchaseOrder>
    {
        Task<List<PurchaseOrderViewModel>> GetBySupplierAsync(int supplierId);
        Task<List<PurchaseOrderViewModel>> GetByStatusAsync(string status);
        Task<List<PurchaseOrderViewModel>> GetRecentAsync(int count = 10);
        Task<PurchaseOrderViewModel> GetByIdWithDetailsAsync(int id);
    }
} 