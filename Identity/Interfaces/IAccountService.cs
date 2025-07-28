using Identity.DTOs;

namespace Identity.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de cuenta
    /// </summary>
    public interface IAccountService
    {
        /// <summary>
        /// Autentica un usuario
        /// </summary>
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        Task<RegisterResponse> RegisterBasicAsync(RegisterRequest request, string origin);

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        Task<List<AuthenticationResponse>> GetAllUsersAsync();

        /// <summary>
        /// Obtiene un usuario por nombre de usuario
        /// </summary>
        Task<AuthenticationResponse> GetUserByNameAsync(string userName);

        /// <summary>
        /// Actualiza un usuario
        /// </summary>
        Task<AuthenticationResponse> UpdateUser(AuthenticationResponse vm);

        /// <summary>
        /// Cierra la sesión
        /// </summary>
        Task SignOutAsync();
    }
} 