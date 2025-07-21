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
        public List<SaleDetailViewModel> Details { get; set; }
        public PaymentMethodType PaymentMethod { get; set; }
        public PaymentViewModel Payment { get; set; }
        
        // Propiedades calculadas para el dashboard
        public decimal Total => TotalAmount;
        public int TotalItems { get; set; }
    }
}
