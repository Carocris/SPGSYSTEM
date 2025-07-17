using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class SaleDetail
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SaleId { get; set; }
        public int Quantity { get; set; }

        public decimal Subtotal { get; set; }

        // Navigation
        public Sale Sale { get; set; }
        public Product Product { get; set; }
    }
}
