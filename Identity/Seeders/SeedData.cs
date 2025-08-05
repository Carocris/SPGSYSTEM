using Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Database.Contexts;

namespace Database.Seeders
{
    /// <summary>
    /// Clase principal para ejecutar todos los seeds del sistema
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Ejecuta todos los seeds del sistema
        /// </summary>
        /// <param name="userManager">Gestor de usuarios</param>
        /// <param name="roleManager">Gestor de roles</param>
        /// <param name="applicationDbContext">Contexto de la aplicación</param>
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext applicationDbContext)
        {
            try
            {
                Console.WriteLine("Iniciando proceso de seeds...");

                // 1. Crear roles por defecto
                await DefaultRoles.SeedAsync(roleManager);

                // 2. Crear usuario administrador
                await DefaultAdminUser.SeedAsync(userManager);

                // 3. Crear usuarios de prueba
                await DefaultUsers.SeedAsync(userManager);

                // 4. Crear proveedor por defecto
                await DefaultSupplier.SeedAsync(applicationDbContext);

                // 5. Asignar productos a proveedores
                await ProductSupplierSeeder.SeedAsync(applicationDbContext);

                Console.WriteLine("Seeds completados exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en SeedData.SeedAsync: {ex.Message}");
            }
        }
    }
} 