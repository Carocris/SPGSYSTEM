using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class Sale
    {
        public int Id { get; set; }

        [Required]
        public DateTime SaleDate { get; set; }

        public int CustomerId { get; set; }

        public decimal TotalAmount { get; set; }

        // Navigation
        public Customer Customer { get; set; }
        public ICollection<SaleDetail> Details { get; set; }
        public Payment Payment { get; set; }
    }
}
