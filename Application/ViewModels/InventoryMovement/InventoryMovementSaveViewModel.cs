using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.InventoryMovement
{
    /// <summary>
    /// ViewModel para crear/editar movimientos de inventario
    /// </summary>
    public class InventoryMovementSaveViewModel
    {
        [Required(ErrorMessage = "El producto es requerido")]
        [Display(Name = "Producto")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es requerido")]
        [Display(Name = "Tipo de Movimiento")]
        public string MovementType { get; set; } = string.Empty;

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "El motivo es requerido")]
        [StringLength(200, ErrorMessage = "El motivo no puede exceder 200 caracteres")]
        [Display(Name = "Motivo")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        [Display(Name = "Notas")]
        public string? Notes { get; set; }

        [StringLength(100, ErrorMessage = "El número de referencia no puede exceder 100 caracteres")]
        [Display(Name = "Número de Referencia")]
        public string? ReferenceNumber { get; set; }

        [StringLength(50, ErrorMessage = "El tipo de referencia no puede exceder 50 caracteres")]
        [Display(Name = "Tipo de Referencia")]
        public string? ReferenceType { get; set; }

        // Propiedades para mostrar información del producto
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public int? CurrentStock { get; set; }
    }
} 