using Database.Enum;
using Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Database.Seeders
{
    /// Seeds para usuarios de prueba con diferentes roles
    public static class DefaultUsers
    {
        /// Crea usuarios de prueba con diferentes roles
        /// <param name="userManager">Gestor de usuarios</param>
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            try
            {
                // Usuario Auditor
                await CreateUserAsync(userManager, 
                    "auditor", 
                    "auditor@spgsystem.com", 
                    "Auditor", 
                    "Sistema", 
                    "8095550003", 
                    "Auditor123!", 
                    Roles.Auditor);

                // Usuario Proveedor
                await CreateUserAsync(userManager, 
                    "supplier", 
                    "supplier@spgsystem.com", 
                    "Proveedor", 
                    "Empresa ABC", 
                    "8095550004", 
                    "Supplier123!", 
                    Roles.Supplier);

                // Usuario Cliente
                await CreateUserAsync(userManager, 
                    "customer", 
                    "customer@spgsystem.com", 
                    "Cliente", 
                    "Final", 
                    "8095550005", 
                    "Customer123!", 
                    Roles.Customer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en DefaultUsers.SeedAsync: {ex.Message}");
            }
        }

        /// Método auxiliar para crear usuarios
        private static async Task CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string userName,
            string email,
            string firstName,
            string lastName,
            string phoneNumber,
            string password,
            Roles role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var newUser = new ApplicationUser
                {
                    UserName = userName,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, role.ToString());
                    Console.WriteLine($"Usuario '{userName}' creado exitosamente con rol '{role}'.");
                }
                else
                {
                    Console.WriteLine($"Error creando usuario '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"El usuario '{userName}' ya existe.");
            }
        }
    }
} 