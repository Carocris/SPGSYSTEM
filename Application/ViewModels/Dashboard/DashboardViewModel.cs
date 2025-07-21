using Application.ViewModels.Product;
using Application.ViewModels.Sale;

namespace Application.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        // Estadísticas generales
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalCustomers { get; set; }
        
        // Estadísticas de inventario
        public decimal TotalInventoryValue { get; set; }
        public int ProductsLowStock { get; set; }
        public int ProductsOutOfStock { get; set; }
        
        // Estadísticas de ventas
        public int TotalSalesToday { get; set; }
        public decimal RevenueToday { get; set; }
        public int TotalSalesThisMonth { get; set; }
        public decimal RevenueThisMonth { get; set; }
        
        // Productos con stock bajo
        public List<ProductViewModel> LowStockProducts { get; set; } = new();
        
        // Ventas recientes
        public List<SaleViewModel> RecentSales { get; set; } = new();
        
        // Datos para gráficos
        public List<CategoryStatsViewModel> CategoryStats { get; set; } = new();
        public List<MonthlySalesViewModel> MonthlySales { get; set; } = new();
        public List<TopProductViewModel> TopProducts { get; set; } = new();
    }
    
    public class CategoryStatsViewModel
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public decimal TotalValue { get; set; }
    }
    
    public class MonthlySalesViewModel
    {
        public string Month { get; set; }
        public int SalesCount { get; set; }
        public decimal Revenue { get; set; }
    }
    
    public class TopProductViewModel
    {
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }
} 