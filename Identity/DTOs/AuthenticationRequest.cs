using System.ComponentModel.DataAnnotations;

namespace Identity.DTOs
{
    /// <summary>
    /// DTO para solicitud de autenticación
    /// </summary>
    public class AuthenticationRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; } = string.Empty;
    }
} 