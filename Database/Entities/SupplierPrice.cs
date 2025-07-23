using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    public class SupplierPrice
    {
        public int Id { get; set; }

        [Required]
        public int SupplierId { get; set; }
        public Supplier Supplier { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; } // Precio que cobra este proveedor por este producto

        [StringLength(20)]
        public string? Currency { get; set; } = "USD"; // Moneda del precio

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? LastUpdated { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; } // Notas adicionales sobre el precio
    }
} 