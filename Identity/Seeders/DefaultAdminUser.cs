using Database.Enum;
using Identity.Entities;
using Microsoft.AspNetCore.Identity;

namespace Database.Seeders
{
    /// <summary>
    /// Seed para el usuario administrador por defecto
    /// </summary>
    public static class DefaultAdminUser
    {
        /// <summary>
        /// Crea el usuario administrador por defecto
        /// </summary>
        /// <param name="userManager">Gestor de usuarios</param>
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            try
            {
                ApplicationUser defaultUser = new();
                defaultUser.UserName = "admin";
                defaultUser.Email = "admin@spgsystem.com";
                            defaultUser.CompanyName = "Admin";
            defaultUser.ContactName = "Sistema";
                defaultUser.PhoneNumber = "8095550000";
                defaultUser.EmailConfirmed = true;
                defaultUser.PhoneNumberConfirmed = true;

                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    var result = await userManager.CreateAsync(defaultUser, "Admin123!");
                    
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                        Console.WriteLine("Usuario administrador creado exitosamente.");
                    }
                    else
                    {
                        Console.WriteLine($"Error creando usuario administrador: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine("El usuario administrador ya existe.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en DefaultAdminUser.SeedAsync: {ex.Message}");
            }
        }
    }
} 