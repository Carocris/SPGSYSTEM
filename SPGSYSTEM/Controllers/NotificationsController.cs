using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AutoMapper;

namespace SPGSYSTEM.Controllers
{
    [Authorize(Roles = "Supplier")]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly ISupplierService _supplierService;
        private readonly IMapper _mapper;

        public NotificationsController(
            INotificationService notificationService,
            ISupplierService supplierService,
            IMapper mapper)
        {
            _notificationService = notificationService;
            _supplierService = supplierService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine($"NotificationsController.Index: Usuario autenticado: {User.Identity?.Name}");
                Console.WriteLine($"NotificationsController.Index: Roles: {string.Join(", ", User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value))}");
                
                // Verificar si el usuario tiene el rol Supplier
                if (!User.IsInRole("Supplier"))
                {
                    Console.WriteLine("NotificationsController.Index: Usuario no tiene rol Supplier");
                    TempData["Error"] = "Acceso denegado. Solo proveedores pueden ver notificaciones.";
                    return RedirectToAction("Index", "Home");
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.Identity?.Name;
                var actualUserId = userIdClaim ?? userName;
                
                Console.WriteLine($"NotificationsController.Index: userIdClaim = {userIdClaim}");
                Console.WriteLine($"NotificationsController.Index: userName = {userName}");
                Console.WriteLine($"NotificationsController.Index: actualUserId = {actualUserId}");
                
                var supplier = await GetSupplierByUserId(actualUserId);
                if (supplier == null)
                {
                    Console.WriteLine("NotificationsController.Index: No se pudo identificar el proveedor");
                    TempData["Error"] = "No se pudo identificar tu proveedor.";
                    return RedirectToAction("Index", "Home");
                }

                var notifications = await _notificationService.GetBySupplierIdAsync(supplier.Id);
                var viewModels = _mapper.Map<List<Application.ViewModels.Notification.NotificationViewModel>>(notifications);

                return View(viewModels);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en NotificationsController.Index: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["Error"] = "Error al cargar las notificaciones.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Unread()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var supplier = await GetSupplierByUserId(userIdClaim);
            if (supplier == null)
            {
                TempData["Error"] = "No se pudo identificar tu proveedor.";
                return RedirectToAction("Index", "Home");
            }

            var unreadNotifications = await _notificationService.GetUnreadBySupplierIdAsync(supplier.Id);
            var viewModels = _mapper.Map<List<Application.ViewModels.Notification.NotificationViewModel>>(unreadNotifications);

            return View(viewModels);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var supplier = await GetSupplierByUserId(userIdClaim);
            if (supplier == null)
            {
                TempData["Error"] = "No se pudo identificar tu proveedor.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _notificationService.MarkAsReadAsync(id);
            if (result)
            {
                TempData["Success"] = "Notificación marcada como leída.";
            }
            else
            {
                TempData["Error"] = "No se pudo marcar la notificación como leída.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var supplier = await GetSupplierByUserId(userIdClaim);
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
            {
                Console.WriteLine("GetSupplierByUserId: userId es null o vacío");
                return null;
            }

            try
            {
                Console.WriteLine($"GetSupplierByUserId: Buscando proveedor para userId: {userId}");
                var supplier = await _supplierService.GetByUserIdAsync(userId);
                Console.WriteLine($"GetSupplierByUserId: Resultado - {(supplier != null ? $"ID: {supplier.Id}, Name: {supplier.Name}" : "null")}");
                return supplier;
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error getting supplier by userId: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
    }
} 