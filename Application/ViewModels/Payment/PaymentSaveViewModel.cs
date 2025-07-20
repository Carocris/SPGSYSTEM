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
    }
}
