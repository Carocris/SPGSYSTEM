using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.Product
{
    public class AddStockViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad a Agregar")]
        public int QuantityToAdd { get; set; }

        [StringLength(200)]
        [Display(Name = "Motivo (Opcional)")]
        public string? Reason { get; set; }

        [Display(Name = "Nuevo Precio de Compra (Opcional)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal? NewPurchasePrice { get; set; }
    }
} 