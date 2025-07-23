using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SPGSYSTEM.Models;
using Application.Interfaces.Services;
using Application.ViewModels.Dashboard;
using AutoMapper;
using Application.ViewModels.Product;
using Application.ViewModels.Sale;
using Database.Entities;

namespace SPGSYSTEM.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly ICustomerService _customerService;
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;

        public HomeController(
            ILogger<HomeController> logger,
            IProductService productService,
            ICategoryService categoryService,
            ISupplierService supplierService,
            ICustomerService customerService,
            ISaleService saleService,
            IMapper mapper)
        {
            _logger = logger;
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _customerService = customerService;
            _saleService = saleService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboard = new DashboardViewModel();
                
                // Cargar estadísticas generales
                var products = await _productService.GetAllAsync();
                var categories = await _categoryService.GetAllAsync();
                var suppliers = await _supplierService.GetAllAsync();
                var customers = await _customerService.GetAllAsync();
                
                // Cargar ventas con detalles
                var sales = await _saleService.GetAllWithDetailsAsync();
                
                // Estadísticas básicas
                dashboard.TotalProducts = products.Count();
                dashboard.TotalCategories = categories.Count();
                dashboard.TotalSuppliers = suppliers.Count();
                dashboard.TotalCustomers = customers.Count();
                
                // Estadísticas de inventario
                dashboard.TotalInventoryValue = products.Sum(p => p.Stock * p.PurchasePrice);
                dashboard.ProductsLowStock = products.Count(p => p.IsLowStock);
                dashboard.ProductsOutOfStock = products.Count(p => p.Stock <= 0);
                
                // Estadísticas de ventas
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                
                var salesToday = sales.Where(s => s.SaleDate.Date == today);
                var salesThisMonth = sales.Where(s => s.SaleDate >= thisMonth);
                
                dashboard.TotalSalesToday = salesToday.Count();
                dashboard.RevenueToday = salesToday.Sum(s => s.TotalAmount);
                dashboard.TotalSalesThisMonth = salesThisMonth.Count();
                dashboard.RevenueThisMonth = salesThisMonth.Sum(s => s.TotalAmount);
                
                // Productos con stock bajo (top 10)
                var lowStockProducts = products
                    .Where(p => p.IsLowStock)
                    .OrderBy(p => p.Stock)
                    .Take(10)
                    .ToList();
                dashboard.LowStockProducts = _mapper.Map<List<ProductViewModel>>(lowStockProducts);
                
                // Ventas recientes (últimas 5)
                var recentSales = sales
                    .OrderByDescending(s => s.SaleDate)
                    .Take(5)
                    .ToList();
                dashboard.RecentSales = _mapper.Map<List<SaleViewModel>>(recentSales);
                
                // TotalItems se mapea automáticamente desde los detalles de la venta
                
                // Estadísticas por categoría
                dashboard.CategoryStats = categories.Select(c => new CategoryStatsViewModel
                {
                    CategoryName = c.Name ?? "Sin nombre",
                    ProductCount = products.Count(p => p.CategoryId == c.Id),
                    TotalValue = products.Where(p => p.CategoryId == c.Id).Sum(p => p.Stock * p.PurchasePrice)
                }).Where(c => c.ProductCount > 0).ToList();
                
                // Ventas por mes (últimos 6 meses)
                var sixMonthsAgo = today.AddMonths(-6);
                dashboard.MonthlySales = Enumerable.Range(0, 6)
                    .Select(i => {
                        var month = today.AddMonths(-i);
                        var monthStart = new DateTime(month.Year, month.Month, 1);
                        var monthEnd = monthStart.AddMonths(1);
                        var monthSales = sales.Where(s => s.SaleDate >= monthStart && s.SaleDate < monthEnd);
                        
                        return new MonthlySalesViewModel
                        {
                            Month = month.ToString("MMM yyyy"),
                            SalesCount = monthSales.Count(),
                            Revenue = monthSales.Sum(s => s.TotalAmount)
                        };
                    })
                    .Reverse()
                    .ToList();
                
                // Productos más vendidos - manejar si no hay ventas o detalles
                List<TopProductViewModel> productSales = new();
                
                var salesWithDetails = sales?.Where(s => s.Details != null && s.Details.Any()).ToList();
                
                if (salesWithDetails?.Any() == true)
                {
                    productSales = salesWithDetails
                        .SelectMany(s => s.Details)
                        .GroupBy(sd => sd.ProductId)
                        .Select(g => new TopProductViewModel
                        {
                            ProductName = products.FirstOrDefault(p => p.Id == g.Key)?.Name ?? "Desconocido",
                            QuantitySold = g.Sum(sd => sd.Quantity),
                            Revenue = g.Sum(sd => sd.Subtotal)
                        })
                        .OrderByDescending(p => p.QuantitySold)
                        .Take(5)
                        .ToList();
                }
                
                dashboard.TopProducts = productSales;
                
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["Error"] = "Error al cargar el dashboard: " + ex.Message;
                return View(new DashboardViewModel());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
