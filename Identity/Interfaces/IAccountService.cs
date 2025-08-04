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
        /// Registra un nuevo usuario (solo para clientes - registro público)
        /// </summary>
        Task<RegisterResponse> RegisterBasicAsync(RegisterRequest request, string origin);

        /// <summary>
        /// Registra un nuevo proveedor (solo para administradores)
        /// </summary>
        Task<RegisterResponse> RegisterSupplierAsync(RegisterRequest request, string origin);

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        Task<List<AuthenticationResponse>> GetAllUsersAsync();

        /// <summary>
        /// Obtiene un usuario por nombre de usuario
        /// </summary>
        Task<AuthenticationResponse> GetUserByNameAsync(string userName);

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        Task<AuthenticationResponse> GetUserByIdAsync(string userId);

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