using Microsoft.AspNetCore.Identity;

namespace Identity.Entities
{
    public class ApplicationUser : IdentityUser
    {
        /// Nombre de la empresa
        public string CompanyName { get; set; } = string.Empty;

        /// Persona de contacto
        public string ContactName { get; set; } = string.Empty;
    }
} 