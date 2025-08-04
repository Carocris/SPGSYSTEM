using System.ComponentModel.DataAnnotations;

namespace Identity.DTOs
{
    /// <summary>
    /// DTO para solicitud de registro
    /// </summary>
    public class RegisterRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la empresa es requerido")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La persona de contacto es requerida")]
        public string ContactName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
} 