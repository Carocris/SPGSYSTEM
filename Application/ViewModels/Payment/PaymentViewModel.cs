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

        public PaymentStatusType Status { get; set; }

        public string PaymentStatus => GetPaymentStatusDisplayName(Status);

        public string PaymentMethodName => GetPaymentMethodDisplayName(PaymentMethod);

        public string PaymentMethodIcon => GetPaymentMethodIcon(PaymentMethod);

        public string StatusBadgeClass => GetStatusBadgeClass(Status);

        // Campos específicos para pago con tarjeta
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? CardExpiryDate { get; set; }
        public string? CardCVV { get; set; }

        // Campos específicos para transferencia bancaria
        public string? TransferReference { get; set; }
        public string? BankAccount { get; set; }
        public string? TransferReceiptPath { get; set; }

        public string GetPaymentStatusDisplayName(PaymentStatusType status)
        {
            return status switch
            {
                PaymentStatusType.Completed => "Completado",
                PaymentStatusType.Cancelled => "Cancelado",
                _ => "Desconocido"
            };
        }

        private string GetPaymentMethodDisplayName(PaymentMethodType method)
        {
            return method switch
            {
                PaymentMethodType.Cash => "Efectivo",
                PaymentMethodType.Card => "Tarjeta",
                PaymentMethodType.Transfer => "Transferencia",
                _ => method.ToString()
            };
        }

        private string GetPaymentMethodIcon(PaymentMethodType method)
        {
            return method switch
            {
                PaymentMethodType.Cash => "💵",
                PaymentMethodType.Card => "💳",
                PaymentMethodType.Transfer => "🏦",
                _ => "❓"
            };
        }

        private string GetStatusBadgeClass(PaymentStatusType status)
        {
            return status switch
            {
                PaymentStatusType.Completed => "bg-success",
                PaymentStatusType.Cancelled => "bg-danger",
                _ => "bg-secondary"
            };
        }

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
