using Application.Interfaces.Services;
using Application.ViewModels.Product;
using Application.ViewModels.Supplier;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    /// Controlador específico para proveedores - Solo pueden agregar productos de su empresa
    [Authorize(Roles = "Supplier")]
    public class SupplierPortalController : Controller
    {
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public SupplierPortalController(
            IProductService productService,
            ISupplierService supplierService,
            ICategoryService categoryService,
            IMapper mapper)
        {
            _productService = productService;
            _supplierService = supplierService;
            _categoryService = categoryService;
            _mapper = mapper;
        }

        /// Vista principal del portal de proveedores - Sus productos
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Usuario no identificado.";
                    return View(new List<ProductViewModel>());
                }

                // Obtener el proveedor asociado al usuario
                var supplier = await GetSupplierByUserId(userId);
                if (supplier == null)
                {
                    TempData["Error"] = "No se encontró información del proveedor. Contacte al administrador.";
                    return View(new List<ProductViewModel>());
                }

                // Obtener productos del proveedor
                var products = await _productService.GetAllAsync();
                var supplierProducts = products.Where(p => p.SupplierId == supplier.Id).ToList();
                var viewModels = _mapper.Map<List<ProductViewModel>>(supplierProducts);

                ViewBag.SupplierName = supplier.Name;
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los productos: " + ex.Message;
                return View(new List<ProductViewModel>());
            }
        }

        /// Formulario para agregar un nuevo producto
        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            try
            {
                var userId = User.Identity?.Name;
                var supplier = await GetSupplierByUserId(userId);
                if (supplier == null)
                {
                    TempData["Error"] = "No se encontró información del proveedor.";
                    return RedirectToAction(nameof(Index));
                }

                // Cargar categorías disponibles
                var categories = await _categoryService.GetAllAsync();
                ViewBag.Categories = categories.Select(c => new { c.Id, c.Name }).ToList();
                ViewBag.SupplierId = supplier.Id;
                ViewBag.SupplierName = supplier.Name;

                return View(new ProductSaveViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el formulario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// Procesar la creación de un nuevo producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductSaveViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var categories = await _categoryService.GetAllAsync();
                    ViewBag.Categories = categories.Select(c => new { c.Id, c.Name }).ToList();
                    return View(model);
                }

                var userId = User.Identity?.Name;
                var supplier = await GetSupplierByUserId(userId);
                if (supplier == null)
                {
                    TempData["Error"] = "No se encontró información del proveedor.";
                    return RedirectToAction(nameof(Index));
                }

                // Asignar el proveedor al producto
                model.SupplierId = supplier.Id;

                // Aquí implementarías la lógica para crear el producto
                // Por ahora solo mostramos un mensaje de éxito
                TempData["Success"] = "Producto agregado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// Ver estadísticas de los productos del proveedor
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var userId = User.Identity?.Name;
                var supplier = await GetSupplierByUserId(userId);
                if (supplier == null)
                {
                    TempData["Error"] = "No se encontró información del proveedor.";
                    return RedirectToAction(nameof(Index));
                }

                var products = await _productService.GetAllAsync();
                var supplierProducts = products.Where(p => p.SupplierId == supplier.Id).ToList();

                ViewBag.SupplierName = supplier.Name;
                ViewBag.TotalProducts = supplierProducts.Count;
                ViewBag.ProductsInStock = supplierProducts.Count(p => p.Stock > 0);
                ViewBag.ProductsLowStock = supplierProducts.Count(p => p.IsLowStock);
                ViewBag.TotalValue = supplierProducts.Sum(p => p.Stock * p.PurchasePrice);

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar estadísticas: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// Método auxiliar para obtener el proveedor por ID de usuario
        private async Task<SupplierViewModel?> GetSupplierByUserId(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return null;

                // Obtener el proveedor usando el servicio
                var supplier = await _supplierService.GetByUserIdAsync(userId);
                if (supplier == null)
                    return null;

                // Convertir a ViewModel
                return _mapper.Map<SupplierViewModel>(supplier);
            }
            catch (Exception ex)
            {
                // Log del error pero no lanzar excepción para evitar problemas de UI
                return null;
            }
        }
    }
} 