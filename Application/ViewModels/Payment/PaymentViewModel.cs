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
        public string PaymentStatus { get; set; }
    }
}
