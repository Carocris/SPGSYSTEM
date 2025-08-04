using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Application.ViewModels.Product;
using AutoMapper;
using Database.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection; // Added for HttpContext.RequestServices
using Microsoft.AspNetCore.Identity; // Added for UserManager
using Identity.Entities; // Added for ApplicationUser

namespace SPGSYSTEM.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly ISupplierPriceHistoryService _supplierPriceHistoryService;
        private readonly ISaleDetailService _saleDetailService;
        private readonly IMapper _mapper;

        public ProductsController(
            IProductService productService, 
            ICategoryService categoryService, 
            ISupplierService supplierService,
            ISupplierPriceHistoryService supplierPriceHistoryService,
            ISaleDetailService saleDetailService,
            IMapper mapper)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _supplierPriceHistoryService = supplierPriceHistoryService;
            _saleDetailService = saleDetailService;
            _mapper = mapper;
        }

        private async Task LoadViewBagDataAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                var suppliers = await _supplierService.GetAllAsync();

                ViewBag.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

                ViewBag.Suppliers = suppliers.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar datos del ViewBag: {ex.Message}");
                ViewBag.Categories = new List<SelectListItem>();
                ViewBag.Suppliers = new List<SelectListItem>();
            }
        }

        private async Task<string> GenerateProductCodeAsync()
        {
            var products = await _productService.GetAllAsync();
            var maxCode = products
                .Where(p => !string.IsNullOrEmpty(p.Code) && p.Code.StartsWith("PROD"))
                .Select(p => p.Code)
                .OrderByDescending(c => c)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(maxCode))
            {
                return "PROD001";
            }

            var numberPart = maxCode.Substring(4);
            if (int.TryParse(numberPart, out int currentNumber))
            {
                return $"PROD{(currentNumber + 1):D3}";
            }

            return "PROD001";
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Product> products;
                
                // Filtrar productos según el rol del usuario
                if (User.IsInRole("Admin"))
                {
                    // Admin ve todos los productos
                    var allProducts = await _productService.GetAllAsync();
                    products = allProducts.ToList();
                }
                else if (User.IsInRole("Auditor"))
                {
                    // Auditor ve todos los productos (solo lectura)
                    var allProducts = await _productService.GetAllAsync();
                    products = allProducts.ToList();
                }
                else if (User.IsInRole("Supplier"))
                {
                    // Supplier ve solo sus productos
                    try
                    {
                        var supplier = await GetSupplierByUserId(User.Identity?.Name);
                        if (supplier != null)
                        {
                            products = await _productService.GetBySupplierAsync(supplier.Id);
                        }
                        else
                        {
                            TempData["Warning"] = "No se encontró información de proveedor asociada a tu cuenta.";
                            products = new List<Product>();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error obteniendo productos del proveedor: {ex.Message}");
                        TempData["Warning"] = "Error al cargar tus productos. Contacta al administrador.";
                        products = new List<Product>();
                    }
                }
                else if (User.IsInRole("Customer"))
                {
                    // Customer ve solo productos con stock > 0
                    var allProducts = await _productService.GetAllAsync();
                    products = allProducts.Where(p => p.Stock > 0).ToList();
                }
                else
                {
                    // Usuario sin rol específico - no ve nada
                    products = new List<Product>();
                }
                
                var viewModels = _mapper.Map<List<ProductViewModel>>(products);
                return View(viewModels);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ProductsController.Index: {ex.Message}");
                TempData["Error"] = "Error al cargar los productos. Por favor, intenta de nuevo.";
                return View(new List<ProductViewModel>());
            }
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar permisos de acceso
                if (!CanAccessProduct(product))
                {
                    TempData["Error"] = "No tienes permisos para ver este producto.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<ProductViewModel>(product);
                
                // Cargar historial de ventas del producto
                var salesService = HttpContext.RequestServices.GetService<Application.Interfaces.Services.ISaleService>();
                if (salesService != null)
                {
                    var allSales = await salesService.GetAllWithDetailsAsync();
                    var productSales = allSales.Where(s => s.Details.Any(sd => sd.ProductId == id)).ToList();
                    ViewBag.ProductSales = productSales.Take(5).ToList(); // Solo las últimas 5 ventas
                }
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Products/Create
        [HttpGet]
        [Authorize(Roles = "Admin,Supplier")]
        public async Task<IActionResult> Create(int? supplierId = null, int? categoryId = null)
        {
            try
            {
                await LoadViewBagDataAsync();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Producto";

                var vm = new ProductSaveViewModel
                {
                    Code = await GenerateProductCodeAsync(),
                    CategoryId = categoryId,
                    SupplierId = supplierId
                };

                // Si es Supplier, forzar que solo pueda crear productos para su proveedor
                if (User.IsInRole("Supplier"))
                {
                    var supplier = await GetSupplierByUserId(User.Identity?.Name);
                    if (supplier != null)
                    {
                        vm.SupplierId = supplier.Id;
                        ViewBag.SupplierId = supplier.Id;
                        ViewBag.SupplierName = supplier.Name;
                    }
                }

                return View("CreateEdit", vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el formulario: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supplier")]
        public async Task<IActionResult> Create(ProductSaveViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadViewBagDataAsync();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Producto";
                
                // Si es Supplier, establecer información del proveedor
                if (User.IsInRole("Supplier"))
                {
                    var supplier = await GetSupplierByUserId(User.Identity?.Name);
                    if (supplier != null)
                    {
                        ViewBag.SupplierId = supplier.Id;
                        ViewBag.SupplierName = supplier.Name;
                    }
                }
                
                return View("CreateEdit", vm);
            }

            try
            {
                // Si es Supplier, forzar que el producto pertenezca a su proveedor
                if (User.IsInRole("Supplier"))
                {
                    var supplier = await GetSupplierByUserId(User.Identity?.Name);
                    if (supplier != null)
                    {
                        vm.SupplierId = supplier.Id;
                    }
                    else
                    {
                        TempData["Error"] = "No se pudo identificar tu proveedor.";
                        await LoadViewBagDataAsync();
                        ViewBag.IsEdit = false;
                        ViewBag.PageTitle = "Nuevo Producto";
                        
                        // Si es Supplier, establecer información del proveedor
                        if (User.IsInRole("Supplier"))
                        {
                            var currentSupplier = await GetSupplierByUserId(User.Identity?.Name);
                            if (currentSupplier != null)
                            {
                                ViewBag.SupplierId = currentSupplier.Id;
                                ViewBag.SupplierName = currentSupplier.Name;
                            }
                        }
                        
                        return View("CreateEdit", vm);
                    }
                }

                var product = _mapper.Map<Product>(vm);
                
                // Asegurar que las relaciones estén limpias y las propiedades de navegación sean null
                product.Category = null;
                product.Supplier = null;
                product.SaleDetails = null;
                
                // Log para depuración
                Console.WriteLine($"Creando producto: {product.Name}, Código: {product.Code}");
                Console.WriteLine($"CategoryId: {product.CategoryId}, SupplierId: {product.SupplierId}");
                
                await _productService.CreateAsync(product);

                // Registrar precio inicial si el producto tiene proveedor
                if (product.SupplierId.HasValue && product.PurchasePrice > 0)
                {
                    await _supplierPriceHistoryService.CreateHistoryRecordAsync(
                        product.SupplierId.Value,
                        product.Id,
                        0, // Precio anterior (0 para productos nuevos)
                        product.PurchasePrice,
                        "Sistema",
                        $"Precio inicial establecido al crear el producto",
                        "Creación de producto"
                    );
                }

                TempData["Success"] = $"Producto '{product.Name}' creado exitosamente con {product.Stock} unidades en stock.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log detallado del error
                Console.WriteLine($"Error al crear producto: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
                }
                
                TempData["Error"] = "Error al crear el producto: " + ex.Message;
                
                await LoadViewBagDataAsync();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Producto";
                return View("CreateEdit", vm);
            }
        }
        // GET: /Products/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin,Supplier")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Si es Supplier, verificar que el producto pertenece a su proveedor
                if (User.IsInRole("Supplier"))
                {
                    var supplier = await GetSupplierByUserId(User.Identity?.Name);
                    if (supplier == null || product.SupplierId != supplier.Id)
                    {
                        TempData["Error"] = "No tienes permisos para editar este producto.";
                        return RedirectToAction(nameof(Index));
                    }
                    // Establecer información del proveedor para la vista
                    ViewBag.SupplierId = supplier.Id;
                    ViewBag.SupplierName = supplier.Name;
                }

                await LoadViewBagDataAsync();
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Producto";
                ViewBag.ProductId = id;
                ViewBag.OriginalStock = product.Stock;
                
                var vm = _mapper.Map<ProductSaveViewModel>(product);
                return View("CreateEdit", vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

      

        // POST: /Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Supplier")]
        public async Task<IActionResult> Edit(int id, ProductSaveViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadViewBagDataAsync();
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Producto";
                ViewBag.ProductId = id;
                
                // Si es Supplier, establecer información del proveedor
                if (User.IsInRole("Supplier"))
                {
                    var supplier = await GetSupplierByUserId(User.Identity?.Name);
                    if (supplier != null)
                    {
                        ViewBag.SupplierId = supplier.Id;
                        ViewBag.SupplierName = supplier.Name;
                    }
                }
                
                return View("CreateEdit", vm);
            }

            try
            {
                // Verificar que el producto existe
                var existingProduct = await _productService.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Si es Supplier, verificar que el producto pertenece a su proveedor
                if (User.IsInRole("Supplier"))
                {
                    var supplier = await GetSupplierByUserId(User.Identity?.Name);
                    if (supplier == null || existingProduct.SupplierId != supplier.Id)
                    {
                        TempData["Error"] = "No tienes permisos para editar este producto.";
                        return RedirectToAction(nameof(Index));
                    }
                    // Forzar que el producto siga perteneciendo a su proveedor
                    vm.SupplierId = supplier.Id;
                }

                var product = _mapper.Map<Product>(vm);
                product.Id = id; // Asegurar que el ID se mantiene
                
                // Asegurar que las relaciones estén limpias
                product.Category = null;
                product.Supplier = null;
                product.SaleDetails = null;
                
                await _productService.UpdateAsync(product);

                TempData["Success"] = $"Producto '{product.Name}' actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el producto: " + ex.Message;
                
                await LoadViewBagDataAsync();
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Producto";
                ViewBag.ProductId = id;
                return View("CreateEdit", vm);
            }
        }

        // GET: /Products/AddStock/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddStock(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Product = product;
                ViewBag.CurrentStock = product.Stock;
                return View(new AddStockViewModel { ProductId = id, QuantityToAdd = 0 });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Products/AddStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddStock(AddStockViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var prod = await _productService.GetByIdAsync(model.ProductId);
                ViewBag.Product = prod;
                ViewBag.CurrentStock = prod?.Stock ?? 0;
                return View(model);
            }

            try
            {
                var product = await _productService.GetByIdAsync(model.ProductId);
                if (product == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var before = product.Stock;
                var originalPurchasePrice = product.PurchasePrice;
                
                // Actualizar stock
                product.Stock += model.QuantityToAdd;
                
                // Actualizar precio de compra si se especificó un valor válido
                if (model.NewPurchasePrice.HasValue && model.NewPurchasePrice.Value > 0)
                {
                    product.PurchasePrice = model.NewPurchasePrice.Value;
                }
                
                await _productService.UpdateAsync(product);

                // Registrar movimiento de entrada
                var userName = User.Identity?.Name ?? "Sistema";
                // await _inventoryMovementService.RegisterEntryAsync(
                //     model.ProductId, 
                //     model.QuantityToAdd, 
                //     "Agregado de stock manual", 
                //     null, 
                //     "Manual", 
                //     $"Stock agregado desde la interfaz. Precio anterior: ${originalPurchasePrice:N2}", 
                //     userName);

                // Mensaje de éxito
                var successMessage = $"Stock agregado exitosamente. {product.Name}: {before} → {product.Stock} unidades (+{model.QuantityToAdd})";
                
                if (model.NewPurchasePrice.HasValue && model.NewPurchasePrice.Value > 0 && model.NewPurchasePrice.Value != originalPurchasePrice)
                {
                    successMessage += $". Precio de compra actualizado: ${originalPurchasePrice:N2} → ${model.NewPurchasePrice.Value:N2}";
                }

                TempData["Success"] = successMessage;
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al agregar stock: " + ex.Message;
                var prod = await _productService.GetByIdAsync(model.ProductId);
                ViewBag.Product = prod;
                ViewBag.CurrentStock = prod?.Stock ?? 0;
                return View(model);
            }
        }

        // POST: /Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product != null)
                {
                    // Verificar si el producto tiene ventas asociadas
                    var allSales = await _saleDetailService.GetAllAsync();
                    var hasSales = allSales.Any(sd => sd.ProductId == id);
                    
                    if (hasSales)
                    {
                        TempData["Error"] = $"No se puede eliminar '{product.Name}' porque está asociado a ventas.";
                    }
                    else
                    {
                        await _productService.DeleteAsync(id);
                        TempData["Success"] = $"Producto '{product.Name}' eliminado exitosamente.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el producto: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Products/Search?term=...
        [HttpGet]
        public async Task<JsonResult> Search(string term)
        {
            try
            {
                var products = await _productService.GetAllAsync();
                var filteredProducts = products
                    .Where(p => p.Name.ToLower().Contains(term.ToLower()) || 
                               p.Code.ToLower().Contains(term.ToLower()))
                    .Take(10)
                    .Select(p => new
                    {
                        id = p.Id,
                        text = $"{p.Code} - {p.Name} (Stock: {p.Stock})",
                        price = p.Price,
                        stock = p.Stock
                    })
                    .ToList();

                return Json(filteredProducts);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        // GET: /Products/GetCategories
        [HttpGet]
        public async Task<JsonResult> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetActiveAsync();
                var result = categories.Select(c => new { id = c.Id, name = c.Name }).ToList();
                return Json(result);
            }
            catch
            {
                return Json(new List<object>());
            }
        }

        // GET: /Products/GetSuppliers
        [HttpGet]
        public async Task<JsonResult> GetSuppliers()
        {
            try
            {
                var suppliers = await _supplierService.GetAllAsync();
                var result = suppliers.Select(s => new { id = s.Id, name = s.Name }).ToList();
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }

        // Método auxiliar para verificar si el usuario puede acceder a un producto
        private bool CanAccessProduct(Product product)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Auditor"))
                return true;
            
            if (User.IsInRole("Supplier"))
            {
                // Supplier solo puede ver sus propios productos
                var supplier = GetSupplierByUserId(User.Identity?.Name).Result;
                return supplier != null && product.SupplierId == supplier.Id;
            }
            
            if (User.IsInRole("Customer"))
            {
                // Customer solo puede ver productos con stock > 0
                return product.Stock > 0;
            }
            
            return false;
        }

        // Método auxiliar para obtener el proveedor por userId
        private async Task<Supplier?> GetSupplierByUserId(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return null;

            try
            {
                // Obtener el UserId real del usuario autenticado
                var userManager = HttpContext.RequestServices.GetService<Microsoft.AspNetCore.Identity.UserManager<Identity.Entities.ApplicationUser>>();
                if (userManager != null)
                {
                    var user = await userManager.FindByNameAsync(userId);
                    if (user != null)
                    {
                        return await _supplierService.GetByUserIdAsync(user.Id);
                    }
                }
                
                // Fallback: intentar buscar directamente por username
                return await _supplierService.GetByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetSupplierByUserId: {ex.Message}");
                return null;
            }
        }
    }
}
