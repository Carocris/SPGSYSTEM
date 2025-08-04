namespace Identity.DTOs
{
    /// <summary>
    /// DTO para respuesta de autenticación
    /// </summary>
    public class AuthenticationResponse
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public bool HasError { get; set; }
        public string Error { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public string Password { get; set; } = string.Empty;
    }
} 