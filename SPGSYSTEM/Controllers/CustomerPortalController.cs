using Application.Interfaces.Services;
using Application.ViewModels.Product;
using Application.ViewModels.Sale;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SPGSYSTEM.Controllers
{
    /// <summary>
    /// Controlador específico para clientes - Solo pueden ver productos y realizar compras
    /// </summary>
    [Authorize(Roles = "Customer")]
    public class CustomerPortalController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;

        public CustomerPortalController(
            IProductService productService,
            ICategoryService categoryService,
            ISaleService saleService,
            IMapper mapper)
        {
            _productService = productService;
            _categoryService = categoryService;
            _saleService = saleService;
            _mapper = mapper;
        }

        /// <summary>
        /// Vista principal del portal de clientes - Catálogo de productos
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                var availableProducts = products.Where(p => p.Stock > 0).ToList();
                var viewModels = _mapper.Map<List<ProductViewModel>>(availableProducts);
                
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los productos: " + ex.Message;
                return View(new List<ProductViewModel>());
            }
        }

        /// <summary>
        /// Ver detalles de un producto específico
        /// </summary>
        public async Task<IActionResult> ProductDetails(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null || product.Stock <= 0)
                {
                    TempData["Error"] = "Producto no disponible.";
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

        /// <summary>
        /// Ver historial de compras del cliente
        /// </summary>
        public async Task<IActionResult> MyOrders()
        {
            try
            {
                var userId = User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Usuario no identificado.";
                    return RedirectToAction(nameof(Index));
                }

                // Aquí podrías implementar la lógica para obtener las compras del cliente
                // Por ahora retornamos una lista vacía
                return View(new List<SaleViewModel>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar las órdenes: " + ex.Message;
                return View(new List<SaleViewModel>());
            }
        }

        /// <summary>
        /// Buscar productos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return RedirectToAction(nameof(Index));
                }

                var products = await _productService.GetAllAsync();
                var filteredProducts = products
                    .Where(p => p.Stock > 0 && 
                               (p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) || 
                                p.Code.Contains(term, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                var viewModels = _mapper.Map<List<ProductViewModel>>(filteredProducts);
                ViewBag.SearchTerm = term;
                
                return View("Index", viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error en la búsqueda: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 