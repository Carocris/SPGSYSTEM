using Application.Interfaces.Services;
using Application.ViewModels.Product;
using Application.ViewModels.Supplier;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SPGSYSTEM.Controllers
{
    /// Controlador específico para proveedores - Solo pueden agregar productos de su empresa
    [Authorize(Roles = "Supplier")]
    public class SupplierPortalController : Controller
    {
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;
        private readonly ICategoryService _categoryService;
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;

        public SupplierPortalController(
            IProductService productService,
            ISupplierService supplierService,
            ICategoryService categoryService,
            ISaleService saleService,
            IMapper mapper)
        {
            _productService = productService;
            _supplierService = supplierService;
            _categoryService = categoryService;
            _saleService = saleService;
            _mapper = mapper;
        }

        /// Vista principal del portal de proveedores - Sus productos
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine($"SupplierPortalController.Index: Iniciando...");
                Console.WriteLine($"SupplierPortalController.Index: User.Identity?.Name = {User.Identity?.Name}");
                Console.WriteLine($"SupplierPortalController.Index: User.Identity?.IsAuthenticated = {User.Identity?.IsAuthenticated}");
                
                // Obtener el UserId del usuario autenticado (no el UserName)
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                Console.WriteLine($"SupplierPortalController.Index: userIdClaim = {userIdClaim}");
                
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    Console.WriteLine("SupplierPortalController.Index: No se pudo obtener el ID del usuario");
                    TempData["Error"] = "Usuario no identificado.";
                    return View(new List<ProductViewModel>());
                }
                
                Console.WriteLine($"SupplierPortalController.Index: Usando userIdClaim: {userIdClaim}");
                var supplier = await GetSupplierByUserId(userIdClaim);
                if (supplier == null)
                {
                    Console.WriteLine("SupplierPortalController.Index: No se pudo identificar el proveedor");
                    TempData["Error"] = "No se pudo identificar tu proveedor.";
                    return View(new List<ProductViewModel>());
                }

                Console.WriteLine($"SupplierPortalController.Index: Proveedor encontrado - ID: {supplier.Id}, Name: {supplier.Name}");
                var products = await _productService.GetAllAsync();
                var supplierProducts = products.Where(p => p.SupplierId == supplier.Id).ToList();
                var productViewModels = _mapper.Map<List<ProductViewModel>>(supplierProducts);

                Console.WriteLine($"SupplierPortalController.Index: Productos encontrados: {productViewModels.Count}");
                return View(productViewModels);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en SupplierPortalController.Index: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["Error"] = "Error al cargar los productos.";
                return View(new List<ProductViewModel>());
            }
        }

        /// Formulario para agregar un nuevo producto
        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var supplier = await GetSupplierByUserId(userIdClaim);
                if (supplier == null)
                {
                    TempData["Error"] = "No se encontró información del proveedor.";
                    return RedirectToAction(nameof(Index));
                }

                var categories = await _categoryService.GetAllAsync();
                ViewBag.Categories = categories;

                return View(new ProductSaveViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el formulario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesar la creación de un nuevo producto
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductSaveViewModel model)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var supplier = await GetSupplierByUserId(userIdClaim);
                if (supplier == null)
                {
                    TempData["Error"] = "No se encontró información del proveedor.";
                    return RedirectToAction(nameof(Index));
                }

                model.SupplierId = supplier.Id;

                if (ModelState.IsValid)
                {
                    var success = await _productService.CreateAsync(model);
                    if (success)
                    {
                        TempData["Success"] = "Producto agregado exitosamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Error"] = "Error al agregar el producto.";
                    }
                }

                var categories = await _categoryService.GetAllAsync();
                ViewBag.Categories = categories;

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al agregar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Ver estadísticas de los productos del proveedor
        /// </summary>
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var supplier = await GetSupplierByUserId(userIdClaim);
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

        /// <summary>
        /// Ver las ventas de los productos del proveedor
        /// </summary>
        public async Task<IActionResult> Sales()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var supplier = await GetSupplierByUserId(userIdClaim);
                if (supplier == null)
                {
                    TempData["Error"] = "No se encontró información del proveedor.";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener las ventas del proveedor usando el servicio de ventas
                var sales = await _saleService.GetSalesBySupplierAsync(supplier.Id);
                
                // Convertir a ViewModels para mostrar en la vista
                var saleViewModels = _mapper.Map<List<Application.ViewModels.Sale.SaleViewModel>>(sales);

                ViewBag.SupplierName = supplier.Name;
                ViewBag.SupplierId = supplier.Id;
                ViewBag.TotalSales = saleViewModels.Count;
                ViewBag.TotalRevenue = saleViewModels.Sum(s => s.TotalAmount);

                return View(saleViewModels);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en SupplierPortalController.Sales: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["Error"] = "Error al cargar las ventas.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Método auxiliar para obtener el proveedor por ID de usuario
        /// </summary>
        private async Task<SupplierViewModel?> GetSupplierByUserId(string? userId)
        {
            try
            {
                Console.WriteLine($"GetSupplierByUserId: userId recibido: '{userId}'");
                
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("GetSupplierByUserId: userId es null o vacío");
                    return null;
                }

                // Obtener el proveedor usando el servicio
                var supplier = await _supplierService.GetByUserIdAsync(userId);
                Console.WriteLine($"GetSupplierByUserId: Resultado del servicio - supplier: {(supplier != null ? $"ID: {supplier.Id}, Name: {supplier.Name}" : "null")}");
                
                if (supplier == null)
                {
                    Console.WriteLine("GetSupplierByUserId: No se encontró proveedor en el servicio");
                    return null;
                }

                // Convertir a ViewModel
                var supplierViewModel = _mapper.Map<SupplierViewModel>(supplier);
                Console.WriteLine($"GetSupplierByUserId: ViewModel creado - ID: {supplierViewModel.Id}, Name: {supplierViewModel.Name}");
                return supplierViewModel;
            }
            catch (Exception ex)
            {
                // Log del error pero no lanzar excepción para evitar problemas de UI
                Console.WriteLine($"GetSupplierByUserId: Error - {ex.Message}");
                Console.WriteLine($"GetSupplierByUserId: Stack trace - {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Método auxiliar para obtener el proveedor por UserName
        /// </summary>
        private async Task<SupplierViewModel?> GetSupplierByUserName(string? userName)
        {
            try
            {
                Console.WriteLine($"GetSupplierByUserName: userName recibido: '{userName}'");
                
                if (string.IsNullOrEmpty(userName))
                {
                    Console.WriteLine("GetSupplierByUserName: userName es null o vacío");
                    return null;
                }

                // Obtener el proveedor usando el servicio
                var supplier = await _supplierService.GetByUserNameAsync(userName);
                Console.WriteLine($"GetSupplierByUserName: Resultado del servicio - supplier: {(supplier != null ? $"ID: {supplier.Id}, Name: {supplier.Name}" : "null")}");
                
                if (supplier == null)
                {
                    Console.WriteLine("GetSupplierByUserName: No se encontró proveedor en el servicio");
                    return null;
                }

                // Convertir a ViewModel
                var supplierViewModel = _mapper.Map<SupplierViewModel>(supplier);
                Console.WriteLine($"GetSupplierByUserName: ViewModel creado - ID: {supplierViewModel.Id}, Name: {supplierViewModel.Name}");
                return supplierViewModel;
            }
            catch (Exception ex)
            {
                // Log del error pero no lanzar excepción para evitar problemas de UI
                Console.WriteLine($"GetSupplierByUserName: Error - {ex.Message}");
                Console.WriteLine($"GetSupplierByUserName: Stack trace - {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Método de prueba para verificar la base de datos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                Console.WriteLine("=== PRUEBA DE BASE DE DATOS ===");
                
                // Obtener todos los proveedores
                var allSuppliers = await _supplierService.GetAllAsync();
                Console.WriteLine($"Total de proveedores en BD: {allSuppliers.Count}");
                
                foreach (var supplier in allSuppliers)
                {
                    Console.WriteLine($"- ID: {supplier.Id}, Name: {supplier.Name}, UserId: '{supplier.UserId}'");
                }
                
                // Obtener información del usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.Identity?.Name;
                Console.WriteLine($"Usuario actual - NameIdentifier: '{userIdClaim}', Name: '{userName}'");
                
                // Buscar específicamente el proveedor con el UserId del usuario actual
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    var supplierWithUserId = allSuppliers.FirstOrDefault(s => s.UserId == userIdClaim);
                    Console.WriteLine($"Proveedor con UserId '{userIdClaim}': {(supplierWithUserId != null ? $"ID: {supplierWithUserId.Id}, Name: {supplierWithUserId.Name}" : "NO ENCONTRADO")}");
                }
                
                return Content($"Prueba completada. Revisa los logs en la consola.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en TestDatabase: {ex.Message}");
                return Content($"Error: {ex.Message}");
            }
        }
    }
} 