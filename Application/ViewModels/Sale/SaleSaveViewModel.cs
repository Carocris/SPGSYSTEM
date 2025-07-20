using Application.ViewModels.SaleDetail;
using Database.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Sale
{
    public class SaleSaveViewModel
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
    }
}
