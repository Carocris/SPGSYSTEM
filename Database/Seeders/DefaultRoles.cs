using Database.Enum;
using Microsoft.AspNetCore.Identity;

namespace Database.Seeders
{
    /// Seeds para roles por defecto del sistema
    public static class DefaultRoles
    {
        /// Crea los roles por defecto del sistema
        /// <param name="roleManager">Gestor de roles</param>
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            try
            {
                // Crear todos los roles del sistema
                var roles = Enum.GetValues(typeof(Roles)).Cast<Roles>();

                foreach (var role in roles)
                {
                    var roleName = role.ToString();
                    var roleExists = await roleManager.RoleExistsAsync(roleName);

                    if (!roleExists)
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        Console.WriteLine($"Rol '{roleName}' creado exitosamente.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando roles: {ex.Message}");
            }
        }
    }
} 