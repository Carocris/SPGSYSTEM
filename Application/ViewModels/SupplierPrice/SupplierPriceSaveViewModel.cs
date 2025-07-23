using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.SupplierPrice
{
    public class SupplierPriceSaveViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El proveedor es requerido")]
        [Display(Name = "Proveedor")]
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "El producto es requerido")]
        [Display(Name = "Producto")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio")]
        public decimal Price { get; set; }

        [StringLength(20)]
        [Display(Name = "Moneda")]
        public string Currency { get; set; } = "USD";

        [Display(Name = "Activo")]
        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Notas")]
        public string? Notes { get; set; }

        // Propiedades para mostrar en la vista
        [Display(Name = "Nombre del Proveedor")]
        public string? SupplierName { get; set; }

        [Display(Name = "Nombre del Producto")]
        public string? ProductName { get; set; }

        [Display(Name = "Código del Producto")]
        public string? ProductCode { get; set; }
    }
} 