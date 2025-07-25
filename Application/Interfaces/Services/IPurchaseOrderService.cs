using System.Collections.Generic;
using System.Threading.Tasks;
using Application.ViewModels.PurchaseOrder;
using Database.Entities;

namespace Application.Interfaces.Services
{
    public interface IPurchaseOrderService : IGenericService<PurchaseOrder>
    {
        Task<List<PurchaseOrderViewModel>> GetBySupplierAsync(int supplierId);
        Task<List<PurchaseOrderViewModel>> GetByStatusAsync(string status);
        Task<List<PurchaseOrderViewModel>> GetRecentAsync(int count = 10);
        Task<PurchaseOrderViewModel> GetByIdWithDetailsAsync(int id);
        Task<bool> UpdateStatusAsync(int id, string newStatus);
        Task<bool> UpdateDeliveryDateAsync(int id, DateTime deliveryDate);
        Task<bool> UpdateReceivedQuantityAsync(int detailId, int receivedQuantity);
    }
} 