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
using Microsoft.Extensions.DependencyInjection; // Added for HttpContext.RequestServices

namespace SPGSYSTEM.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly ISupplierPriceHistoryService _supplierPriceHistoryService;
        private readonly IMapper _mapper;

        public ProductsController(
            IProductService productService, 
            ICategoryService categoryService, 
            ISupplierService supplierService,
            ISupplierPriceHistoryService supplierPriceHistoryService,
            IMapper mapper)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _supplierPriceHistoryService = supplierPriceHistoryService;
            _mapper = mapper;
        }

        private async Task LoadViewBagDataAsync()
        {
            try
            {
                var categories = await _categoryService.GetActiveAsync();
                var suppliers = await _supplierService.GetActiveAsync();

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
                Console.WriteLine($"Error cargando datos del ViewBag: {ex.Message}");
            }
        }

        // GET: /Products
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                var viewModels = _mapper.Map<List<ProductViewModel>>(products);
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los productos: " + ex.Message;
                return View(new List<ProductViewModel>());
            }
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _productService.GetWithDetailsAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var vm = _mapper.Map<ProductViewModel>(product);
                
                // Cargar historial de ventas del producto
                var salesService = HttpContext.RequestServices.GetService<Application.Interfaces.Services.ISaleService>();
                if (salesService != null)
                {
                    var allSales = await salesService.GetAllWithDetailsAsync();
                    var productSales = allSales
                        .Where(s => s.Details != null && s.Details.Any(d => d.ProductId == id))
                        .OrderByDescending(s => s.SaleDate)
                        .Take(10)
                        .ToList();
                    
                    var productSalesViewModels = _mapper.Map<List<Application.ViewModels.Sale.SaleViewModel>>(productSales);
                    ViewBag.ProductSales = productSalesViewModels;
                }
                
                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Products/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? supplierId = null, int? categoryId = null)
        {
            try
            {
                await LoadViewBagDataAsync();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Producto";
                
                var viewModel = new ProductSaveViewModel();
                
                // Si se proporciona un supplierId, preseleccionar el proveedor
                if (supplierId.HasValue)
                {
                    viewModel.SupplierId = supplierId.Value;
                    ViewBag.SelectedSupplierId = supplierId.Value;
                }
                
                // Si se proporciona un categoryId, preseleccionar la categoría
                if (categoryId.HasValue)
                {
                    viewModel.CategoryId = categoryId.Value;
                    ViewBag.SelectedCategoryId = categoryId.Value;
                }
                
                return View("CreateEdit", viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los datos: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductSaveViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadViewBagDataAsync();
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Producto";
                return View("CreateEdit", vm);
            }

            try
            {
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
        public async Task<IActionResult> Edit(int id, ProductSaveViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadViewBagDataAsync();
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Producto";
                ViewBag.ProductId = id;
                return View("CreateEdit", vm);
            }

            try
            {
                var existing = await _productService.GetByIdAsync(id);
                if (existing == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Guardar valores originales para comparar cambios
                var originalStock = existing.Stock;
                var originalPurchasePrice = existing.PurchasePrice;
                var originalSalePrice = existing.SalePrice;

                _mapper.Map(vm, existing);
                await _productService.UpdateAsync(existing);

                // Registrar cambios de precios si el producto tiene proveedor
                if (existing.SupplierId.HasValue)
                {
                    var priceChanges = new List<string>();

                    // Verificar cambio en precio de compra
                    if (originalPurchasePrice != existing.PurchasePrice)
                    {
                        await _supplierPriceHistoryService.CreateHistoryRecordAsync(
                            existing.SupplierId.Value,
                            existing.Id,
                            originalPurchasePrice,
                            existing.PurchasePrice,
                            "Sistema",
                            $"Actualización de precio de compra desde la edición del producto",
                            "Actualización manual"
                        );
                        priceChanges.Add($"Precio de compra: ${originalPurchasePrice:N2} → ${existing.PurchasePrice:N2}");
                    }

                    // Verificar cambio en precio de venta
                    if (originalSalePrice != existing.SalePrice)
                    {
                        await _supplierPriceHistoryService.CreateHistoryRecordAsync(
                            existing.SupplierId.Value,
                            existing.Id,
                            originalSalePrice,
                            existing.SalePrice,
                            "Sistema",
                            $"Actualización de precio de venta desde la edición del producto",
                            "Actualización manual"
                        );
                        priceChanges.Add($"Precio de venta: ${originalSalePrice:N2} → ${existing.SalePrice:N2}");
                    }

                    // Agregar mensaje sobre cambios de precios
                    if (priceChanges.Any())
                    {
                        TempData["Info"] = $"Cambios de precios registrados: {string.Join(", ", priceChanges)}";
                    }
                }

                var delta = existing.Stock - originalStock;
                var stockMsg = delta > 0
                    ? $" Stock incrementado en {delta} unidades (ahora: {existing.Stock})."
                    : delta < 0
                        ? $" Stock reducido en {Math.Abs(delta)} unidades (ahora: {existing.Stock})."
                        : "";
                TempData["Success"] = $"Producto '{existing.Name}' actualizado exitosamente.{stockMsg}";

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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product != null)
                {
                    var withDetails = await _productService.GetWithDetailsAsync(id);
                    if (withDetails.SaleDetails?.Any() == true)
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
                var result = products
                    .Where(p => p.Name.Contains(term ?? "", StringComparison.OrdinalIgnoreCase))
                    .Take(10)
                    .Select(p => new { id = p.Id, name = p.Name, price = p.Price, stock = p.Stock })
                    .ToList();
                return Json(result);
            }
            catch
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
                var suppliers = await _supplierService.GetActiveAsync();
                var result = suppliers.Select(s => new { id = s.Id, name = s.Name }).ToList();
                return Json(result);
            }
            catch
            {
                return Json(new List<object>());
            }
        }
    }
}
