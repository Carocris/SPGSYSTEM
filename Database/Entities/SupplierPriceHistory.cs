using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    public class SupplierPriceHistory
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
        public decimal OldPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal NewPrice { get; set; }

        [StringLength(20)]
        public string? Currency { get; set; } = "USD";

        public DateTime ChangeDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ChangedBy { get; set; } // Usuario que hizo el cambio

        [StringLength(500)]
        public string? Notes { get; set; } // Notas sobre el cambio

        [StringLength(50)]
        public string? ChangeReason { get; set; } // Razón del cambio (actualización, corrección, etc.)
    }
} 