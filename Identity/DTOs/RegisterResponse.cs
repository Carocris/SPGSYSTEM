namespace Identity.DTOs
{
    /// <summary>
    /// DTO para respuesta de registro
    /// </summary>
    public class RegisterResponse
    {
        public bool HasError { get; set; }
        public string Error { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public bool Success => !HasError;
        public string Message { get; set; } = string.Empty;
    }
} 