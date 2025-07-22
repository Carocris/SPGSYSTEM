using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Entities
{
    public class Supplier
    {
        public Supplier()
        {
            Products = new List<Product>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? TaxId { get; set; } // RUC/NIT/Tax ID

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation: Un proveedor puede proveer muchos productos
        public ICollection<Product> Products { get; set; }
    }
} 