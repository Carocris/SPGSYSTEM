namespace Identity.DTOs
{
    /// <summary>
    /// DTO para respuesta de registro
    /// </summary>
    public class RegisterResponse
    {
        public bool HasError { get; set; }
        public string Error { get; set; } = string.Empty;
    }
} 