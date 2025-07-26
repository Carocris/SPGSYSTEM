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
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public List<SaleDetailViewModel> Details { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
        public PaymentViewModel Payment { get; set; }

        // Computed properties for backward compatibility
        public decimal Total => TotalAmount;
        public int TotalItems => Details?.Sum(d => d.Quantity) ?? 0;
        public string PaymentStatus => Payment?.PaymentStatus ?? "Sin pago";
        public string PaymentStatusDisplay => Payment?.PaymentStatus ?? "Sin pago";
        public string PaymentStatusBadgeClass => Payment?.StatusBadgeClass ?? "bg-secondary";
        public bool IsPaid => Payment?.Status == Database.Enum.PaymentStatusType.Completed;
        public bool IsCancelled => Payment?.Status == Database.Enum.PaymentStatusType.Cancelled;
    }

}
