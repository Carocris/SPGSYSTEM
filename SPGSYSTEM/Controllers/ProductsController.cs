using Application.Interfaces.Services;
using Application.ViewModels.Product;
using AutoMapper;
using Database.Entities;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductsController(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        // GET: Products
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
                // Log error here
                TempData["Error"] = "Error al cargar los productos: " + ex.Message;
                return View(new List<ProductViewModel>());
            }
        }

        // GET: Products/Details/5
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

                var viewModel = _mapper.Map<ProductViewModel>(product);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Products/CreateEdit (para crear)
        public IActionResult CreateEdit()
        {
            ViewBag.IsEdit = false;
            ViewBag.PageTitle = "Nuevo Producto";
            return View(new ProductSaveViewModel());
        }

        // GET: Products/CreateEdit/5 (para editar)
        public async Task<IActionResult> CreateEdit(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<ProductSaveViewModel>(product);
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Producto";
                ViewBag.ProductId = id;
                ViewBag.OriginalStock = product.Stock; // Para mostrar cambios de stock
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Products/CreateEdit (crear)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(ProductSaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Producto";
                return View(viewModel);
            }

            try
            {
                var product = _mapper.Map<Product>(viewModel);
                await _productService.CreateAsync(product);
                
                TempData["Success"] = $"Producto '{product.Name}' creado exitosamente con {product.Stock} unidades en stock.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear el producto: " + ex.Message;
                ViewBag.IsEdit = false;
                ViewBag.PageTitle = "Nuevo Producto";
                return View(viewModel);
            }
        }

        // POST: Products/CreateEdit/5 (editar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(int id, ProductSaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Producto";
                ViewBag.ProductId = id;
                return View(viewModel);
            }

            try
            {
                var existingProduct = await _productService.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var originalStock = existingProduct.Stock;
                
                // Mapear los cambios al producto existente
                _mapper.Map(viewModel, existingProduct);
                
                await _productService.UpdateAsync(existingProduct);
                
                // Mensaje específico sobre cambios de stock
                var stockChange = existingProduct.Stock - originalStock;
                string stockMessage = stockChange switch
                {
                    > 0 => $" Stock incrementado en {stockChange} unidades (ahora: {existingProduct.Stock}).",
                    < 0 => $" Stock reducido en {Math.Abs(stockChange)} unidades (ahora: {existingProduct.Stock}).",
                    0 => "",
                    _ => ""
                };
                
                TempData["Success"] = $"Producto '{existingProduct.Name}' actualizado exitosamente.{stockMessage}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el producto: " + ex.Message;
                ViewBag.IsEdit = true;
                ViewBag.PageTitle = "Editar Producto";
                ViewBag.ProductId = id;
                return View(viewModel);
            }
        }

        // GET: Products/AddStock/5 (agregar stock)
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

        // POST: Products/AddStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock(AddStockViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var product = await _productService.GetByIdAsync(model.ProductId);
                ViewBag.Product = product;
                ViewBag.CurrentStock = product?.Stock ?? 0;
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

                var previousStock = product.Stock;
                product.Stock += model.QuantityToAdd;
                
                await _productService.UpdateAsync(product);
                
                TempData["Success"] = $"Stock agregado exitosamente. {product.Name}: {previousStock} → {product.Stock} unidades (+{model.QuantityToAdd})";
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al agregar stock: " + ex.Message;
                var product = await _productService.GetByIdAsync(model.ProductId);
                ViewBag.Product = product;
                ViewBag.CurrentStock = product?.Stock ?? 0;
                return View(model);
            }
        }

        // Mantener compatibilidad con métodos anteriores
        public IActionResult Create() => CreateEdit();
        public async Task<IActionResult> Edit(int id) => await CreateEdit(id);

        // POST: Products/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    TempData["Error"] = "Producto no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si el producto está siendo usado en ventas
                var productWithDetails = await _productService.GetWithDetailsAsync(id);
                if (productWithDetails?.SaleDetails?.Any() == true)
                {
                    TempData["Error"] = $"No se puede eliminar el producto '{product.Name}' porque está asociado a ventas existentes.";
                    return RedirectToAction(nameof(Index));
                }

                await _productService.DeleteAsync(product.Id);
                
                TempData["Success"] = $"Producto '{product.Name}' eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el producto: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // API endpoint para búsqueda rápida (opcional)
        [HttpGet]
        public async Task<JsonResult> Search(string term)
        {
            try
            {
                var products = await _productService.GetAllAsync();
                var filteredProducts = products
                    .Where(p => p.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .Take(10)
                    .Select(p => new { id = p.Id, name = p.Name, price = p.Price, stock = p.Stock })
                    .ToList();

                return Json(filteredProducts);
            }
            catch
            {
                return Json(new List<object>());
            }
        }
    }
} 