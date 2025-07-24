using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Enum;

namespace Database.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public int SaleId { get; set; }

        [Required]
        public PaymentMethodType PaymentMethod { get; set; }

        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public PaymentStatusType Status { get; set; } = PaymentStatusType.Completed;

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

        [StringLength(255)]
        public string? TransferReceiptPath { get; set; }

        // Navigation
        public Sale Sale { get; set; }
    }
}
