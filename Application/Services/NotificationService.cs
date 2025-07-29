using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.ViewModels.Notification;
using AutoMapper;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IMapper _mapper;

        public NotificationService(
            INotificationRepository notificationRepository,
            ISupplierRepository supplierRepository,
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _supplierRepository = supplierRepository;
            _mapper = mapper;
        }

        public async Task<bool> NotifySupplierOfSaleAsync(int supplierId, string productName, int quantity, string customerName, int saleId)
        {
            try
            {
                var notification = new Notification
                {
                    SupplierId = supplierId,
                    Title = "Nueva Venta de Producto",
                    Message = $"Se vendió {quantity} unidad(es) de '{productName}' al cliente '{customerName}'",
                    Type = "Sale",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    SaleId = saleId,
                    ProductName = productName,
                    Quantity = quantity,
                    CustomerName = customerName
                };

                await _notificationRepository.AddAsync(notification);
                await _notificationRepository.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                // En un sistema real, esto iría a un sistema de logging
                Console.WriteLine($"Error al enviar notificación al proveedor {supplierId}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<NotificationViewModel>> GetBySupplierIdAsync(int supplierId)
        {
            var notifications = await _notificationRepository.GetBySupplierIdAsync(supplierId);
            return _mapper.Map<List<NotificationViewModel>>(notifications);
        }

        public async Task<List<NotificationViewModel>> GetUnreadBySupplierIdAsync(int supplierId)
        {
            var notifications = await _notificationRepository.GetUnreadBySupplierIdAsync(supplierId);
            return _mapper.Map<List<NotificationViewModel>>(notifications);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            try
            {
                await _notificationRepository.MarkAsReadAsync(notificationId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 