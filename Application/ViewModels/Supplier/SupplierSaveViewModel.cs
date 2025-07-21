using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Supplier
{
    public class SupplierSaveViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "El nombre de contacto no puede exceder 100 caracteres")]
        public string? ContactPerson { get; set; }

        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        public string? Phone { get; set; }

        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string? Email { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string? Address { get; set; }

        [StringLength(50, ErrorMessage = "La ciudad no puede exceder 50 caracteres")]
        public string? City { get; set; }

        [StringLength(20, ErrorMessage = "El código postal no puede exceder 20 caracteres")]
        public string? PostalCode { get; set; }

        [StringLength(50, ErrorMessage = "El país no puede exceder 50 caracteres")]
        public string? Country { get; set; }

        [StringLength(20, ErrorMessage = "El ID fiscal no puede exceder 20 caracteres")]
        public string? TaxId { get; set; }

        public bool IsActive { get; set; } = true;
    }
} 