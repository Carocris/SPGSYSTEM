using Application.Interfaces.Services;
using Application.ViewModels.InventoryMovement;
using AutoMapper;
using Database.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    /// <summary>
    /// Controlador para gestión de movimientos de inventario
    /// </summary>
    [Authorize]
    public class InventoryMovementsController : Controller
    {
        private readonly IInventoryMovementService _inventoryMovementService;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public InventoryMovementsController(
            IInventoryMovementService inventoryMovementService,
            IProductService productService,
            IMapper mapper)
        {
            _inventoryMovementService = inventoryMovementService;
            _productService = productService;
            _mapper = mapper;
        }

        /// <summary>
        /// Lista todos los movimientos de inventario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var movements = await _inventoryMovementService.GetAllAsync();
                var viewModels = _mapper.Map<List<InventoryMovementViewModel>>(movements);
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los movimientos: " + ex.Message;
                return View(new List<InventoryMovementViewModel>());
            }
        }

        /// <summary>
        /// Muestra el formulario para crear un nuevo movimiento
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                ViewBag.Products = products.Select(p => new { p.Id, p.Name, p.Code }).ToList();
                
                return View(new InventoryMovementSaveViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el formulario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la creación de un nuevo movimiento
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Create(InventoryMovementSaveViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var products = await _productService.GetAllAsync();
                ViewBag.Products = products.Select(p => new { p.Id, p.Name, p.Code }).ToList();
                return View(model);
            }

            try
            {
                var userName = User.Identity?.Name ?? "Sistema";

                InventoryMovement movement;
                switch (model.MovementType)
                {
                    case "Entrada":
                        movement = await _inventoryMovementService.RegisterEntryAsync(
                            model.ProductId, model.Quantity, model.Reason, 
                            model.ReferenceNumber, model.ReferenceType, model.Notes, userName);
                        break;

                    case "Salida":
                        movement = await _inventoryMovementService.RegisterExitAsync(
                            model.ProductId, model.Quantity, model.Reason, 
                            model.ReferenceNumber, model.ReferenceType, model.Notes, userName);
                        break;

                    case "Ajuste":
                        movement = await _inventoryMovementService.RegisterAdjustmentAsync(
                            model.ProductId, model.Quantity, model.Reason, model.Notes, userName);
                        break;

                    default:
                        ModelState.AddModelError("MovementType", "Tipo de movimiento no válido");
                        var products = await _productService.GetAllAsync();
                        ViewBag.Products = products.Select(p => new { p.Id, p.Name, p.Code }).ToList();
                        return View(model);
                }

                TempData["Success"] = $"Movimiento registrado exitosamente. Stock actualizado.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var products = await _productService.GetAllAsync();
                ViewBag.Products = products.Select(p => new { p.Id, p.Name, p.Code }).ToList();
                return View(model);
            }
        }

        /// <summary>
        /// Muestra los detalles de un movimiento
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var movement = await _inventoryMovementService.GetByIdAsync(id);
                if (movement == null)
                {
                    TempData["Error"] = "Movimiento no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<InventoryMovementViewModel>(movement);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los detalles: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Filtra movimientos por producto
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ByProduct(int productId)
        {
            try
            {
                var movements = await _inventoryMovementService.GetByProductAsync(productId);
                var viewModels = _mapper.Map<List<InventoryMovementViewModel>>(movements);
                
                var product = await _productService.GetByIdAsync(productId);
                ViewBag.ProductName = product?.Name ?? "Producto";
                
                return View("Index", viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al filtrar movimientos: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Filtra movimientos por fecha
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ByDate(DateTime startDate, DateTime endDate)
        {
            try
            {
                var movements = await _inventoryMovementService.GetByDateRangeAsync(startDate, endDate);
                var viewModels = _mapper.Map<List<InventoryMovementViewModel>>(movements);
                
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                
                return View("Index", viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al filtrar movimientos: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Obtiene información de un producto para el formulario
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetProductInfo(int productId)
        {
            try
            {
                var product = await _productService.GetByIdAsync(productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }

                return Json(new
                {
                    success = true,
                    productName = product.Name,
                    productCode = product.Code,
                    currentStock = product.Stock
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 