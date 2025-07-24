using Application.ViewModels.SaleDetail;
using Application.ViewModels.Payment;
using Database.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Sale
{
    public class SaleViewModel
    {
        public int Id { get; set; }
        public DateTime SaleDate { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public int CustomerId { get; set; }
        public ICollection<SaleDetailViewModel> Details { get; set; }
        public PaymentViewModel? Payment { get; set; }
        
        // Payment status properties
        public PaymentStatusType PaymentStatus { get; set; }

        public string PaymentStatusDisplay => GetPaymentStatusDisplayName(PaymentStatus);

        public string PaymentStatusBadgeClass => GetPaymentStatusBadgeClass(PaymentStatus);

        public bool IsPaid => PaymentStatus == PaymentStatusType.Completed;
        public bool IsCancelled => PaymentStatus == PaymentStatusType.Cancelled;

        private string GetPaymentStatusDisplayName(PaymentStatusType status)
        {
            return status switch
            {
                PaymentStatusType.Completed => "Completado",
                PaymentStatusType.Cancelled => "Cancelado",
                _ => status.ToString()
            };
        }

        private string GetPaymentStatusBadgeClass(PaymentStatusType status)
        {
            return status switch
            {
                PaymentStatusType.Completed => "bg-success",
                PaymentStatusType.Cancelled => "bg-danger",
                _ => "bg-secondary"
            };
        }
        
        // Propiedades calculadas para el dashboard
        public decimal Total => TotalAmount;
        public int TotalItems { get; set; }
    }
}
