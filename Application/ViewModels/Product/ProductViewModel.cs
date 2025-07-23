using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Product
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        
        // Información de categoría
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        
        // Información de proveedor
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        
        // Precios
        public decimal PurchasePrice { get; set; }
        public decimal SalePrice { get; set; }
        

        public int Stock { get; set; }
        public int MinimumStock { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        
        // Propiedades calculadas
        public decimal ProfitMargin { get; set; }
        public bool IsLowStock { get; set; }
        public decimal InventoryValue { get; set; }
        
        // Compatibilidad con código existente
        public decimal Price => SalePrice;
        
        // Propiedades adicionales para la vista
        public string StockStatus => IsLowStock ? "Stock Bajo" : Stock <= 0 ? "Sin Stock" : "Disponible";
        public string StockBadgeClass => IsLowStock ? "bg-warning" : Stock <= 0 ? "bg-danger" : "bg-success";
    }
}
