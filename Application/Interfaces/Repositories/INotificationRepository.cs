using Database.Entities;

namespace Application.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<Notification>> GetBySupplierIdAsync(int supplierId);
        Task<List<Notification>> GetUnreadBySupplierIdAsync(int supplierId);
        Task MarkAsReadAsync(int notificationId);
    }
} 