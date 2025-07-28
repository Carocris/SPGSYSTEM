using Identity.DTOs;
using Identity.Entities;
using Identity.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Services
{
    /// <summary>
    /// Servicio para gestión de cuentas de usuario
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Autentica un usuario
        /// </summary>
        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            AuthenticationResponse response = new();

            var user = await _userManager.FindByNameAsync(request.UserName);

            if (user == null)
            {
                response.HasError = true;
                response.Error = $"No hay cuenta registrada con {request.UserName}";
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = $"Credenciales inválidas para {request.UserName}";
                return response;
            }

            if (!user.EmailConfirmed)
            {
                response.HasError = true;
                response.Error = $"Cuenta no confirmada para {request.UserName}";
                return response;
            }

            response.Id = user.Id;
            response.Email = user.Email;
            response.UserName = user.UserName;
            response.FirstName = user.FirstName;
            response.LastName = user.LastName;
            response.PhoneNumber = user.PhoneNumber;

            var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            response.Roles = rolesList.ToList();
            response.IsVerified = user.EmailConfirmed;

            return response;
        }

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        public async Task<List<AuthenticationResponse>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();

            var responseList = users.Select(user => new AuthenticationResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsVerified = user.EmailConfirmed,
                HasError = false
            }).ToList();

            return responseList;
        }

        /// <summary>
        /// Obtiene un usuario por nombre de usuario
        /// </summary>
        public async Task<AuthenticationResponse> GetUserByNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return null;
            }

            AuthenticationResponse response = new AuthenticationResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                IsVerified = user.EmailConfirmed,
                HasError = false,
            };

            return response;
        }

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        public async Task<RegisterResponse> RegisterBasicAsync(RegisterRequest request, string origin)
        {
            RegisterResponse response = new()
            {
                HasError = false
            };

            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);

            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Error = $"El nombre de usuario {request.UserName} ya está en uso.";
                return response;
            }

            var userWithSameUserEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameUserEmail != null)
            {
                response.HasError = true;
                response.Error = $"El email {request.Email} ya está registrado.";
                return response;
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = $"Ocurrió un error al registrar el usuario.";
                return response;
            }

            return response;
        }

        /// <summary>
        /// Cierra la sesión
        /// </summary>
        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        /// <summary>
        /// Actualiza un usuario
        /// </summary>
        public async Task<AuthenticationResponse> UpdateUser(AuthenticationResponse vm)
        {
            var user = await _userManager.FindByIdAsync(vm.Id);

            if (user == null)
            {
                return null;
            }

            user.UserName = vm.UserName;
            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.Email = vm.Email;
            user.PhoneNumber = vm.PhoneNumber;

            if (!string.IsNullOrEmpty(vm.Password))
            {
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    return null;
                }

                var addPasswordResult = await _userManager.AddPasswordAsync(user, vm.Password);
                if (!addPasswordResult.Succeeded)
                {
                    return null;
                }
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                vm.UserName = user.UserName;
                vm.FirstName = user.FirstName;
                vm.LastName = user.LastName;
                vm.Email = user.Email;
                vm.PhoneNumber = user.PhoneNumber;

                return vm;
            }

            return null;
        }
    }
} 