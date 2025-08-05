using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Database.Contexts;
using Database.Entities;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetBySupplierIdAsync(int supplierId)
        {
            return await _db.Notifications
                .Where(n => n.SupplierId == supplierId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadBySupplierIdAsync(int supplierId)
        {
            return await _db.Notifications
                .Where(n => n.SupplierId == supplierId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _db.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public override async Task AddAsync(Notification entity)
        {
            Console.WriteLine($"Agregando notificación: {entity.Title} para proveedor {entity.SupplierId}");
            await base.AddAsync(entity);
            Console.WriteLine($"Notificación agregada con ID: {entity.Id}");
        }
    }
} 