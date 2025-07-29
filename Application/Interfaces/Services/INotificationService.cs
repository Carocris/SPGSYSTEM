using System.Threading.Tasks;
using Application.ViewModels.Notification;
using Database.Entities;

namespace Application.Interfaces.Services
{
    public interface INotificationService
    {
        /// <summary>
        /// Notifica al proveedor sobre una venta de su producto
        /// </summary>
        /// <param name="supplierId">ID del proveedor</param>
        /// <param name="productName">Nombre del producto vendido</param>
        /// <param name="quantity">Cantidad vendida</param>
        /// <param name="customerName">Nombre del cliente</param>
        /// <param name="saleId">ID de la venta</param>
        /// <returns>True si la notificación se envió correctamente</returns>
        Task<bool> NotifySupplierOfSaleAsync(int supplierId, string productName, int quantity, string customerName, int saleId);
        
        /// <summary>
        /// Obtiene las notificaciones de un proveedor
        /// </summary>
        /// <param name="supplierId">ID del proveedor</param>
        /// <returns>Lista de notificaciones</returns>
        Task<List<NotificationViewModel>> GetBySupplierIdAsync(int supplierId);
        
        /// <summary>
        /// Obtiene las notificaciones no leídas de un proveedor
        /// </summary>
        /// <param name="supplierId">ID del proveedor</param>
        /// <returns>Lista de notificaciones no leídas</returns>
        Task<List<NotificationViewModel>> GetUnreadBySupplierIdAsync(int supplierId);
        
        /// <summary>
        /// Marca una notificación como leída
        /// </summary>
        /// <param name="notificationId">ID de la notificación</param>
        /// <returns>True si se marcó como leída</returns>
        Task<bool> MarkAsReadAsync(int notificationId);
    }
} 