using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class Product
    {
        public Product()
        {
            SaleDetails = new List<SaleDetail>();
            SupplierPrices = new List<SupplierPrice>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; } // Código único del producto

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Relación con Categoría
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        // Relación con Proveedor
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PurchasePrice { get; set; } // Precio de compra

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal SalePrice { get; set; } // Precio de venta

        [StringLength(20)]
        public string? UnitOfMeasure { get; set; } // Unidad de medida (kg, pz, lt, etc.)

        [Required]
        public int Stock { get; set; } // Stock actual

        [Required]
        public int MinimumStock { get; set; } = 5; // Stock mínimo configurable

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? LastUpdated { get; set; }

        // Navigation: One product can appear in many sale details
        public ICollection<SaleDetail> SaleDetails { get; set; }

        // Navigation: One product can have many supplier prices
        public ICollection<SupplierPrice> SupplierPrices { get; set; } = new List<SupplierPrice>();

        // Propiedades calculadas
        [NotMapped]
        public decimal ProfitMargin => SalePrice > 0 ? ((SalePrice - PurchasePrice) / SalePrice) * 100 : 0;

        [NotMapped]
        public bool IsLowStock => Stock <= MinimumStock;

        [NotMapped]
        public decimal InventoryValue => Stock * PurchasePrice;

        // Compatibilidad con código existente
        [NotMapped]
        public decimal Price => SalePrice;
    }
}
