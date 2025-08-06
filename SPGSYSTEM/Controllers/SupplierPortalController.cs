using Application.Interfaces.Services;
using Application.ViewModels.Product;
using Application.ViewModels.Supplier;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Identity.Interfaces;
using Identity.DTOs;

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
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public SupplierPortalController(
            IProductService productService,
            ISupplierService supplierService,
            ICategoryService categoryService,
            ISaleService saleService,
            IAccountService accountService,
            IMapper mapper)
        {
            _productService = productService;
            _supplierService = supplierService;
            _categoryService = categoryService;
            _saleService = saleService;
            _accountService = accountService;
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
                
                // Obtener el UserName del usuario autenticado
                var userName = User.Identity?.Name;
                
                Console.WriteLine($"SupplierPortalController.Index: userName = {userName}");
                
                if (string.IsNullOrEmpty(userName))
                {
                    Console.WriteLine("SupplierPortalController.Index: No se pudo obtener el nombre del usuario");
                    TempData["Error"] = "Usuario no identificado.";
                    return View(new List<ProductViewModel>());
                }
                
                Console.WriteLine($"SupplierPortalController.Index: Usando userName: {userName}");
                
                // El usuario autenticado ES el proveedor, no necesitamos buscar una entidad Supplier separada
                // Solo verificamos que tenga el rol Supplier
                if (!User.IsInRole("Supplier"))
                {
                    Console.WriteLine("SupplierPortalController.Index: Usuario no tiene rol Supplier");
                    TempData["Error"] = "Acceso denegado. Solo proveedores pueden acceder a este portal.";
                    return RedirectToAction("Index", "Home");
                }

                Console.WriteLine($"SupplierPortalController.Index: Usuario autenticado como proveedor: {userName}");
                
                // Obtener productos que pertenecen a este proveedor (usuario)
                var products = await _productService.GetAllAsync();
                Console.WriteLine($"SupplierPortalController.Index: Total de productos en BD: {products.Count}");
                
                // Debug: Mostrar todos los productos y sus suplidores
                foreach (var product in products.Take(5))
                {
                    Console.WriteLine($"Producto: {product.Name}, SupplierId: {product.SupplierId}, Supplier?.UserId: {product.Supplier?.UserId}, Supplier?.Name: {product.Supplier?.Name}");
                }
                
                // Obtener el proveedor por userName para obtener su ID
                var supplier = await _supplierService.GetByUserNameAsync(userName);
                Console.WriteLine($"SupplierPortalController.Index: Proveedor encontrado para {userName}: {(supplier != null ? $"ID: {supplier.Id}, UserId: {supplier.UserId}" : "null")}");
                
                if (supplier != null)
                {
                    // Filtrar productos por el ID del proveedor
                    var supplierProducts = products.Where(p => p.SupplierId == supplier.Id).ToList();
                    Console.WriteLine($"SupplierPortalController.Index: Productos filtrados para proveedor ID {supplier.Id}: {supplierProducts.Count}");
                    
                    var productViewModels = _mapper.Map<List<ProductViewModel>>(supplierProducts);
                    Console.WriteLine($"SupplierPortalController.Index: Productos encontrados: {productViewModels.Count}");
                    return View(productViewModels);
                }
                else
                {
                    Console.WriteLine($"SupplierPortalController.Index: No se encontró proveedor para {userName}");
                    TempData["Error"] = "No se encontró información de proveedor asociada a tu cuenta. Contacta al administrador.";
                    return View(new List<ProductViewModel>());
                }
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
                var userName = User.Identity?.Name;
                
                // Verificar que el usuario tenga rol Supplier
                if (!User.IsInRole("Supplier"))
                {
                    TempData["Error"] = "Acceso denegado. Solo proveedores pueden agregar productos.";
                    return RedirectToAction("Index", "Home");
                }

                if (string.IsNullOrEmpty(userName))
                {
                    TempData["Error"] = "No se pudo identificar el usuario.";
                    return RedirectToAction(nameof(Index));
                }

                var categories = await _categoryService.GetAllAsync();
                ViewBag.Categories = categories;
                ViewBag.UserName = userName; // Pasar el nombre de usuario para usarlo en el formulario

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
                var userName = User.Identity?.Name;
                
                // Verificar que el usuario tenga rol Supplier
                if (!User.IsInRole("Supplier"))
                {
                    TempData["Error"] = "Acceso denegado. Solo proveedores pueden agregar productos.";
                    return RedirectToAction("Index", "Home");
                }

                if (string.IsNullOrEmpty(userName))
                {
                    TempData["Error"] = "No se pudo identificar el usuario.";
                    return RedirectToAction(nameof(Index));
                }

                // Buscar el proveedor por el nombre de usuario
                var supplier = await _supplierService.GetByUserNameAsync(userName);
                if (supplier == null)
                {
                    // Si no existe el proveedor, crearlo automáticamente
                    var user = await _accountService.GetUserByNameAsync(userName);
                    if (user != null)
                    {
                        var supplierModel = new Application.ViewModels.Supplier.SupplierSaveViewModel
                        {
                            Name = user.CompanyName ?? $"Empresa de {userName}",
                            ContactPerson = user.ContactName ?? "Proveedor",
                            Email = user.Email,
                            Phone = user.PhoneNumber,
                            UserId = user.Id,
                            IsActive = true,
                            Address = "Dirección por definir",
                            City = "Ciudad por definir",
                            Country = "República Dominicana",
                            PostalCode = "00000",
                            TaxId = "00000000000"
                        };
                        
                        var success = await _supplierService.CreateAsync(supplierModel);
                        if (success)
                        {
                            supplier = await _supplierService.GetByUserNameAsync(userName);
                        }
                    }
                }

                if (supplier != null)
                {
                    model.SupplierId = supplier.Id;
                }
                else
                {
                    TempData["Error"] = "No se pudo crear la información del proveedor.";
                    return RedirectToAction(nameof(Index));
                }

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
                var userName = User.Identity?.Name;
                var supplier = await GetSupplierByUserName(userName);
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
                var userName = User.Identity?.Name;
                var supplier = await GetSupplierByUserName(userName);
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
                Console.WriteLine($"GetSupplierByUserName: Resultado del servicio - supplier: {(supplier != null ? $"ID: {supplier.Id}, Name: {supplier.Name}, UserId: {supplier.UserId}" : "null")}");
                
                if (supplier == null)
                {
                    Console.WriteLine("GetSupplierByUserName: No se encontró proveedor en el servicio");
                    
                    // Intentar buscar directamente en la base de datos
                    var allSuppliers = await _supplierService.GetAllAsync();
                    Console.WriteLine($"GetSupplierByUserName: Total de proveedores en BD: {allSuppliers.Count}");
                    foreach (var s in allSuppliers)
                    {
                        Console.WriteLine($"- ID: {s.Id}, Name: {s.Name}, UserId: '{s.UserId}'");
                    }
                    
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
        /// Método de diagnóstico para verificar usuarios y suplidores
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Diagnostico()
        {
            try
            {
                Console.WriteLine("=== DIAGNÓSTICO DE USUARIOS Y SUPLIDORES ===");
                
                // Obtener todos los proveedores
                var allSuppliers = await _supplierService.GetAllAsync();
                Console.WriteLine($"Total de proveedores en BD: {allSuppliers.Count}");
                
                foreach (var supplier in allSuppliers)
                {
                    Console.WriteLine($"- ID: {supplier.Id}, Name: {supplier.Name}, UserId: '{supplier.UserId}'");
                }
                
                // Obtener información del usuario actual
                var userName = User.Identity?.Name;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRoles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                
                Console.WriteLine($"Usuario actual - Name: '{userName}'");
                Console.WriteLine($"Usuario actual - NameIdentifier: '{userIdClaim}'");
                Console.WriteLine($"Usuario actual - Roles: {string.Join(", ", userRoles)}");
                
                // Buscar específicamente el proveedor con el UserName del usuario actual
                if (!string.IsNullOrEmpty(userName))
                {
                    var supplierWithUserName = allSuppliers.FirstOrDefault(s => s.UserId == userName);
                    Console.WriteLine($"Proveedor con UserId '{userName}': {(supplierWithUserName != null ? $"ID: {supplierWithUserName.Id}, Name: {supplierWithUserName.Name}" : "NO ENCONTRADO")}");
                }
                
                // Probar el método GetByUserNameAsync directamente
                if (!string.IsNullOrEmpty(userName))
                {
                    var supplierFromService = await _supplierService.GetByUserNameAsync(userName);
                    Console.WriteLine($"Resultado de GetByUserNameAsync('{userName}'): {(supplierFromService != null ? $"ID: {supplierFromService.Id}, Name: {supplierFromService.Name}" : "null")}");
                }
                
                // Verificar productos por suplidor
                Console.WriteLine("=== PRODUCTOS POR SUPLIDOR ===");
                foreach (var supplier in allSuppliers)
                {
                    var products = await _productService.GetAllAsync();
                    var supplierProducts = products.Where(p => p.SupplierId == supplier.Id).ToList();
                    Console.WriteLine($"Suplidor '{supplier.Name}' (ID: {supplier.Id}): {supplierProducts.Count} productos");
                    
                    foreach (var product in supplierProducts.Take(3)) // Mostrar solo los primeros 3
                    {
                        Console.WriteLine($"  - Producto: {product.Name} (Stock: {product.Stock})");
                    }
                }
                
                // Verificar usuarios de Identity
                Console.WriteLine("=== USUARIOS DE IDENTITY ===");
                var allUsers = await _accountService.GetAllUsersAsync();
                Console.WriteLine($"Total de usuarios en Identity: {allUsers.Count}");
                
                foreach (var user in allUsers)
                {
                    Console.WriteLine($"- ID: '{user.Id}', UserName: '{user.UserName}', CompanyName: '{user.CompanyName}', Roles: {string.Join(", ", user.Roles ?? new List<string>())}");
                }
                
                return Content($"Diagnóstico completado. Revisa los logs en la consola. Usuario: {userName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Diagnostico: {ex.Message}");
                return Content($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Método para verificar la estructura de usuarios del sistema
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> VerificarUsuarios()
        {
            try
            {
                Console.WriteLine("=== VERIFICACIÓN DE ESTRUCTURA DE USUARIOS ===");
                
                // Obtener todos los usuarios de Identity
                var allUsers = await _accountService.GetAllUsersAsync();
                Console.WriteLine($"Total de usuarios en Identity: {allUsers.Count}");
                
                // Categorizar usuarios por rol
                var admins = new List<AuthenticationResponse>();
                var auditors = new List<AuthenticationResponse>();
                var suppliers = new List<AuthenticationResponse>();
                var customers = new List<AuthenticationResponse>();
                
                foreach (var user in allUsers)
                {
                    if (user.Roles != null)
                    {
                        if (user.Roles.Contains("Admin"))
                            admins.Add(user);
                        else if (user.Roles.Contains("Auditor"))
                            auditors.Add(user);
                        else if (user.Roles.Contains("Supplier"))
                            suppliers.Add(user);
                        else if (user.Roles.Contains("Customer"))
                            customers.Add(user);
                    }
                }
                
                Console.WriteLine($"=== RESUMEN DE USUARIOS ===");
                Console.WriteLine($"- Admins: {admins.Count}");
                foreach (var admin in admins)
                {
                    Console.WriteLine($"  * {admin.UserName} ({admin.CompanyName})");
                }
                
                Console.WriteLine($"- Auditors: {auditors.Count}");
                foreach (var auditor in auditors)
                {
                    Console.WriteLine($"  * {auditor.UserName} ({auditor.CompanyName})");
                }
                
                Console.WriteLine($"- Suppliers: {suppliers.Count}");
                foreach (var supplier in suppliers)
                {
                    Console.WriteLine($"  * {supplier.UserName} ({supplier.CompanyName})");
                }
                
                Console.WriteLine($"- Customers: {customers.Count}");
                foreach (var customer in customers)
                {
                    Console.WriteLine($"  * {customer.UserName} ({customer.CompanyName})");
                }
                
                // Verificar productos por suplidor
                Console.WriteLine($"=== PRODUCTOS POR SUPLIDOR ===");
                var products = await _productService.GetAllAsync();
                
                foreach (var supplier in suppliers)
                {
                    // Buscar productos que tengan el UserName del suplidor como SupplierId
                    var supplierProducts = products.Where(p => p.Supplier?.UserId == supplier.UserName || p.Supplier?.Name == supplier.UserName).ToList();
                    Console.WriteLine($"Suplidor '{supplier.UserName}' ({supplier.CompanyName}): {supplierProducts.Count} productos");
                    
                    foreach (var product in supplierProducts.Take(3)) // Mostrar solo los primeros 3
                    {
                        Console.WriteLine($"  - Producto: {product.Name} (Stock: {product.Stock})");
                    }
                }
                
                return Content($"Verificación completada. Revisa los logs en la consola.\n\n" +
                             $"Admins: {admins.Count}\n" +
                             $"Auditors: {auditors.Count}\n" +
                             $"Suppliers: {suppliers.Count}\n" +
                             $"Customers: {customers.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en VerificarUsuarios: {ex.Message}");
                return Content($"Error: {ex.Message}");
            }
        }
    }
} 