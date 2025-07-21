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

        // GET: Products/Create
        public IActionResult Create()
        {
            return View(new ProductSaveViewModel());
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductSaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var product = _mapper.Map<Product>(viewModel);
                await _productService.CreateAsync(product);
                
                TempData["Success"] = $"Producto '{product.Name}' creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear el producto: " + ex.Message;
                return View(viewModel);
            }
        }

        // GET: Products/Edit/5
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

                var viewModel = _mapper.Map<ProductSaveViewModel>(product);
                ViewBag.ProductId = id;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el producto: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductSaveViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
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

                // Mapear los cambios al producto existente
                _mapper.Map(viewModel, existingProduct);
                
                await _productService.UpdateAsync(existingProduct);
                
                TempData["Success"] = $"Producto '{existingProduct.Name}' actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el producto: " + ex.Message;
                ViewBag.ProductId = id;
                return View(viewModel);
            }
        }

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