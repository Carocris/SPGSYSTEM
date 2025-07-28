using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Entities
{
    /// <summary>
    /// Entidad para registrar todos los movimientos de inventario
    /// </summary>
    public class InventoryMovement
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public string MovementType { get; set; } // "Entrada", "Salida", "Ajuste"
        
        [Required]
        public int Quantity { get; set; } // Cantidad positiva para entradas, negativa para salidas
        
        [Required]
        public int StockBefore { get; set; } // Stock antes del movimiento
        
        [Required]
        public int StockAfter { get; set; } // Stock después del movimiento

        [Required]
        [StringLength(200)]
        public string Reason { get; set; } // Motivo del movimiento

        [StringLength(500)]
        public string? Notes { get; set; } // Notas adicionales

        [StringLength(100)]
        public string? ReferenceNumber { get; set; } // Número de factura, orden, etc.

        [StringLength(50)]
        public string? ReferenceType { get; set; } // "Factura", "Orden de Compra", "Ajuste Manual", etc.

        [Required]
        public DateTime MovementDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? CreatedBy { get; set; } // Usuario responsable

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Propiedades calculadas
        [NotMapped]
        public bool IsEntry => MovementType == "Entrada" || MovementType == "Ajuste Positivo";
        
        [NotMapped]
        public bool IsExit => MovementType == "Salida" || MovementType == "Ajuste Negativo";
        
        [NotMapped]
        public string MovementTypeBadge => IsEntry ? "bg-success" : "bg-danger";
    }
} 