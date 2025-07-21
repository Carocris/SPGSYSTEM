using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Product
{
    public class ProductSaveViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "El código es requerido")]
        [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
        public string Code { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        public string? Description { get; set; }

        [Display(Name = "Categoría")]
        public int? CategoryId { get; set; }

        [Display(Name = "Proveedor")]
        public int? SupplierId { get; set; }

        [Required(ErrorMessage = "El precio de compra es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de compra debe ser mayor a 0")]
        [Display(Name = "Precio de Compra")]
        public decimal PurchasePrice { get; set; }

        [Required(ErrorMessage = "El precio de venta es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor a 0")]
        [Display(Name = "Precio de Venta")]
        public decimal SalePrice { get; set; }

        [StringLength(20, ErrorMessage = "La unidad de medida no puede exceder 20 caracteres")]
        [Display(Name = "Unidad de Medida")]
        public string? UnitOfMeasure { get; set; }

        [Required(ErrorMessage = "El stock es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "El stock mínimo es requerido")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        [Display(Name = "Stock Mínimo")]
        public int MinimumStock { get; set; } = 5;

        public bool IsActive { get; set; } = true;

        // Validación personalizada
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (SalePrice <= PurchasePrice)
            {
                results.Add(new ValidationResult(
                    "El precio de venta debe ser mayor al precio de compra.",
                    new[] { nameof(SalePrice) }));
            }

            if (MinimumStock > Stock)
            {
                results.Add(new ValidationResult(
                    "El stock mínimo no puede ser mayor al stock actual.",
                    new[] { nameof(MinimumStock) }));
            }

            return results;
        }

        // Compatibilidad con código existente
        public decimal Price => SalePrice;
    }
}
