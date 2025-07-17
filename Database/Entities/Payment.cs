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

        // Navigation
        public Sale Sale { get; set; }
    }
}
