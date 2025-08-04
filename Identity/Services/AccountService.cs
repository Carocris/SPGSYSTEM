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

            Console.WriteLine($"AccountService.AuthenticateAsync: Intentando autenticar con: {request.UserName}");

            // Intentar buscar por UserName primero
            var user = await _userManager.FindByNameAsync(request.UserName);
            Console.WriteLine($"AccountService.AuthenticateAsync: Búsqueda por UserName '{request.UserName}': {(user != null ? "ENCONTRADO" : "NO ENCONTRADO")}");

            // Si no se encuentra por UserName, intentar por Email
            if (user == null)
            {
                Console.WriteLine($"AccountService.AuthenticateAsync: Intentando búsqueda por Email: {request.UserName}");
                user = await _userManager.FindByEmailAsync(request.UserName);
                Console.WriteLine($"AccountService.AuthenticateAsync: Búsqueda por Email '{request.UserName}': {(user != null ? "ENCONTRADO" : "NO ENCONTRADO")}");
            }

            if (user == null)
            {
                response.HasError = true;
                response.Error = $"No hay cuenta registrada con el usuario o email: {request.UserName}";
                Console.WriteLine($"AccountService.AuthenticateAsync: Usuario no encontrado. Error: {response.Error}");
                return response;
            }

            Console.WriteLine($"AccountService.AuthenticateAsync: Usuario encontrado. UserName: {user.UserName}, Email: {user.Email}, EmailConfirmed: {user.EmailConfirmed}");

            // Verificar la contraseña usando UserManager
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            Console.WriteLine($"AccountService.AuthenticateAsync: Verificación de contraseña: {passwordValid}");

            if (!passwordValid)
            {
                response.HasError = true;
                response.Error = $"Credenciales inválidas para {request.UserName}";
                Console.WriteLine($"AccountService.AuthenticateAsync: Credenciales inválidas. Error: {response.Error}");
                return response;
            }

            response.Id = user.Id;
            response.Email = user.Email;
            response.UserName = user.UserName;
            response.CompanyName = user.CompanyName;
            response.ContactName = user.ContactName;
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
                CompanyName = user.CompanyName,
                ContactName = user.ContactName,
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
                CompanyName = user.CompanyName,
                ContactName = user.ContactName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                IsVerified = user.EmailConfirmed,
                HasError = false,
            };

            return response;
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        public async Task<AuthenticationResponse> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            AuthenticationResponse response = new AuthenticationResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                CompanyName = user.CompanyName,
                ContactName = user.ContactName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                IsVerified = user.EmailConfirmed,
                HasError = false,
            };

            return response;
        }

        /// <summary>
        /// Registra un nuevo usuario (solo para clientes - registro público)
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
                CompanyName = request.CompanyName,
                ContactName = request.ContactName,
                PhoneNumber = request.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = $"Ocurrió un error al registrar el usuario.";
                return response;
            }

            // Asignar automáticamente el rol "Customer" para registros públicos
            var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
            if (!roleResult.Succeeded)
            {
                response.HasError = true;
                response.Error = $"Usuario creado pero error al asignar rol: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}";
                return response;
            }

            response.UserId = user.Id;
            response.Message = "Usuario registrado exitosamente.";
            return response;
        }

        /// <summary>
        /// Registra un nuevo proveedor (solo para administradores)
        /// </summary>
        public async Task<RegisterResponse> RegisterSupplierAsync(RegisterRequest request, string origin)
        {
            RegisterResponse response = new()
            {
                HasError = false
            };

            Console.WriteLine($"AccountService.RegisterSupplierAsync: Iniciando registro de proveedor");
            Console.WriteLine($"AccountService.RegisterSupplierAsync: UserName: {request.UserName}");
            Console.WriteLine($"AccountService.RegisterSupplierAsync: Email: {request.Email}");
            Console.WriteLine($"AccountService.RegisterSupplierAsync: CompanyName: {request.CompanyName}");
            Console.WriteLine($"AccountService.RegisterSupplierAsync: ContactName: {request.ContactName}");
            Console.WriteLine($"AccountService.RegisterSupplierAsync: PhoneNumber: {request.PhoneNumber}");

            // Validar que los campos requeridos no estén vacíos
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                response.HasError = true;
                response.Error = "El nombre de usuario no puede estar vacío.";
                Console.WriteLine($"AccountService.RegisterSupplierAsync: Error - UserName vacío");
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                response.HasError = true;
                response.Error = "El email no puede estar vacío.";
                Console.WriteLine($"AccountService.RegisterSupplierAsync: Error - Email vacío");
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.CompanyName))
            {
                response.HasError = true;
                response.Error = "El nombre de la empresa no puede estar vacío.";
                Console.WriteLine($"AccountService.RegisterSupplierAsync: Error - CompanyName vacío");
                return response;
            }

            if (string.IsNullOrWhiteSpace(request.ContactName))
            {
                response.HasError = true;
                response.Error = "La persona de contacto no puede estar vacía.";
                Console.WriteLine($"AccountService.RegisterSupplierAsync: Error - ContactName vacío");
                return response;
            }

            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            Console.WriteLine($"AccountService.RegisterSupplierAsync: Verificando UserName '{request.UserName}': {(userWithSameUserName != null ? "YA EXISTE" : "DISPONIBLE")}");

            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Error = $"El nombre de usuario {request.UserName} ya está en uso.";
                Console.WriteLine($"AccountService.RegisterSupplierAsync: Error - UserName ya existe");
                return response;
            }

            var userWithSameUserEmail = await _userManager.FindByEmailAsync(request.Email);
            Console.WriteLine($"AccountService.RegisterSupplierAsync: Verificando Email '{request.Email}': {(userWithSameUserEmail != null ? "YA EXISTE" : "DISPONIBLE")}");

            if (userWithSameUserEmail != null)
            {
                response.HasError = true;
                response.Error = $"El email {request.Email} ya está registrado.";
                Console.WriteLine($"AccountService.RegisterSupplierAsync: Error - Email ya existe");
                return response;
            }

            Console.WriteLine($"AccountService.RegisterSupplierAsync: Creando ApplicationUser");
            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.UserName,
                CompanyName = request.CompanyName,
                ContactName = request.ContactName,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = true, // Confirmar automáticamente el email para proveedores
                PhoneNumberConfirmed = true // Confirmar automáticamente el teléfono para proveedores
            };

            Console.WriteLine($"AccountService.RegisterSupplierAsync: ApplicationUser creado - UserName: {user.UserName}, Email: {user.Email}");

            var result = await _userManager.CreateAsync(user, request.Password);
            Console.WriteLine($"AccountService.RegisterSupplierAsync: Resultado de creación: {result.Succeeded}");

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = $"Ocurrió un error al registrar el proveedor: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                Console.WriteLine($"AccountService.RegisterSupplierAsync: Error en creación - {response.Error}");
                return response;
            }

            // Verificar que el usuario se creó correctamente
            var createdUser = await _userManager.FindByNameAsync(request.UserName);
            Console.WriteLine($"AccountService.RegisterSupplierAsync: Usuario creado verificado: {(createdUser != null ? "ENCONTRADO" : "NO ENCONTRADO")}");
            if (createdUser != null)
            {
                Console.WriteLine($"AccountService.RegisterSupplierAsync: Usuario encontrado - ID: {createdUser.Id}, UserName: {createdUser.UserName}, Email: {createdUser.Email}");
            }

            Console.WriteLine($"AccountService.RegisterSupplierAsync: Usuario creado exitosamente. ID: {user.Id}");

            // Asignar automáticamente el rol "Supplier" para proveedores
            var roleResult = await _userManager.AddToRoleAsync(user, "Supplier");
            Console.WriteLine($"AccountService.RegisterSupplierAsync: Resultado de asignación de rol: {roleResult.Succeeded}");

            if (!roleResult.Succeeded)
            {
                response.HasError = true;
                response.Error = $"Proveedor creado pero error al asignar rol: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}";
                Console.WriteLine($"AccountService.RegisterSupplierAsync: Error en asignación de rol - {response.Error}");
                return response;
            }

            response.UserId = user.Id;
            response.Message = "Proveedor registrado exitosamente.";
            Console.WriteLine($"AccountService.RegisterSupplierAsync: Proveedor registrado exitosamente. UserId: {user.Id}");
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
            user.CompanyName = vm.CompanyName;
            user.ContactName = vm.ContactName;
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
                vm.CompanyName = user.CompanyName;
                vm.ContactName = user.ContactName;
                vm.Email = user.Email;
                vm.PhoneNumber = user.PhoneNumber;

                return vm;
            }

            return null;
        }
    }
} 