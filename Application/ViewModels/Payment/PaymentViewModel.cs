using Database.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Payment
{
    public class PaymentViewModel
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public string SaleNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal SaleTotal { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        // Campos específicos para pago con tarjeta
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? CardExpiryDate { get; set; }
        public string? CardCVV { get; set; }

        // Campos específicos para transferencia bancaria
        public string? TransferReference { get; set; }
        public string? BankAccount { get; set; }
        public string? TransferReceiptPath { get; set; }
        public string PaymentStatus { get; set; }


        // Helper properties
        public string MaskedCardNumber 
        { 
            get 
            {
                if (string.IsNullOrEmpty(CardNumber)) return "";
                
                var cleanNumber = CardNumber.Replace(" ", "");
                if (cleanNumber.Length >= 4)
                {
                    return "**** **** **** " + cleanNumber.Substring(cleanNumber.Length - 4);
                }
                return CardNumber;
            }
        }
    }
}
