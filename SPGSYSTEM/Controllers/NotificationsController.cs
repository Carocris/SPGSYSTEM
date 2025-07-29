using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    [Authorize(Roles = "Supplier")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ISupplierService _supplierService;

        public NotificationsController(
            INotificationService notificationService,
            ISupplierService supplierService)
        {
            _notificationService = notificationService;
            _supplierService = supplierService;
        }

        public async Task<IActionResult> Index()
        {
            var supplier = await GetSupplierByUserId(User.Identity?.Name);
            if (supplier == null)
            {
                TempData["Error"] = "No se pudo identificar tu proveedor.";
                return RedirectToAction("Index", "Home");
            }

            var notifications = await _notificationService.GetBySupplierIdAsync(supplier.Id);
            return View(notifications);
        }

        public async Task<IActionResult> Unread()
        {
            var supplier = await GetSupplierByUserId(User.Identity?.Name);
            if (supplier == null)
            {
                TempData["Error"] = "No se pudo identificar tu proveedor.";
                return RedirectToAction("Index", "Home");
            }

            var notifications = await _notificationService.GetUnreadBySupplierIdAsync(supplier.Id);
            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            if (result)
            {
                TempData["Success"] = "Notificación marcada como leída.";
            }
            else
            {
                TempData["Error"] = "Error al marcar la notificación como leída.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var supplier = await GetSupplierByUserId(User.Identity?.Name);
            if (supplier == null)
            {
                TempData["Error"] = "No se pudo identificar tu proveedor.";
                return RedirectToAction("Index", "Home");
            }

            var unreadNotifications = await _notificationService.GetUnreadBySupplierIdAsync(supplier.Id);
            var successCount = 0;

            foreach (var notification in unreadNotifications)
            {
                var result = await _notificationService.MarkAsReadAsync(notification.Id);
                if (result) successCount++;
            }

            if (successCount > 0)
            {
                TempData["Success"] = $"{successCount} notificación(es) marcada(s) como leída(s).";
            }
            else
            {
                TempData["Info"] = "No hay notificaciones para marcar como leídas.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<Database.Entities.Supplier?> GetSupplierByUserId(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            try
            {
                return await _supplierService.GetByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error getting supplier by userId: {ex.Message}");
                return null;
            }
        }
    }
} 