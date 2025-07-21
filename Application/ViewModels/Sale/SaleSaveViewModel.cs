using Application.ViewModels.SaleDetail;
using Database.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Sale
{
    public class SaleSaveViewModel : IValidatableObject
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Add at least one sale line.")]
        public List<SaleDetailSaveViewModel> Details { get; set; }

        [Required]
        [EnumDataType(typeof(PaymentMethodType))]
        public PaymentMethodType PaymentMethod { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal AmountPaid { get; set; }

        // Campos específicos para pago con tarjeta
        [StringLength(19)]
        public string? CardNumber { get; set; }

        [StringLength(100)]
        public string? CardHolderName { get; set; }

        [StringLength(7)] // MM/YYYY
        public string? CardExpiryDate { get; set; }

        [StringLength(4)]
        public string? CardCVV { get; set; }

        // Campos específicos para transferencia bancaria
        [StringLength(200)]
        public string? TransferReference { get; set; }

        [StringLength(100)]
        public string? BankAccount { get; set; }

        public IFormFile? TransferReceipt { get; set; }

        [StringLength(255)]
        public string? TransferReceiptPath { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Validaciones específicas para pago con tarjeta
            if (PaymentMethod == PaymentMethodType.Card)
            {
                if (string.IsNullOrWhiteSpace(CardNumber))
                    results.Add(new ValidationResult("El número de tarjeta es requerido.", new[] { nameof(CardNumber) }));

                if (string.IsNullOrWhiteSpace(CardHolderName))
                    results.Add(new ValidationResult("El nombre del titular es requerido.", new[] { nameof(CardHolderName) }));

                if (string.IsNullOrWhiteSpace(CardExpiryDate))
                    results.Add(new ValidationResult("La fecha de expiración es requerida.", new[] { nameof(CardExpiryDate) }));

                if (string.IsNullOrWhiteSpace(CardCVV))
                    results.Add(new ValidationResult("El CVV es requerido.", new[] { nameof(CardCVV) }));
            }

            // Validaciones específicas para transferencia
            if (PaymentMethod == PaymentMethodType.Transfer)
            {
                if (string.IsNullOrWhiteSpace(TransferReference))
                    results.Add(new ValidationResult("El número de referencia es requerido.", new[] { nameof(TransferReference) }));

                if (string.IsNullOrWhiteSpace(BankAccount))
                    results.Add(new ValidationResult("El banco origen es requerido.", new[] { nameof(BankAccount) }));
            }

            return results;
        }
    }
}
