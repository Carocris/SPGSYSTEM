using Database.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Payment
{
    public class PaymentSaveViewModel
    {
        [Required]
        public int SaleId { get; set; }

        [Required]
        [EnumDataType(typeof(PaymentMethodType))]
        public PaymentMethodType PaymentMethod { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [EnumDataType(typeof(PaymentStatusType))]
        public PaymentStatusType Status { get; set; } = PaymentStatusType.Completed;

        // Campos para pago con tarjeta
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? CardExpiryDate { get; set; }
        public string? CardCVV { get; set; }

        // Campos para transferencia
        public string? BankAccount { get; set; }
        public string? TransferReference { get; set; }
        public string? TransferReceiptPath { get; set; } // Se usará para el path del archivo
    }
}
