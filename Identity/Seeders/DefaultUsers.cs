using Database.Enum;
using Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Database.Entities;
using Database.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Identity.Seeders
{
    /// Seeds para usuarios de prueba con diferentes roles
    public static class DefaultUsers
    {
        /// Crea usuarios de prueba con diferentes roles
        /// <param name="userManager">Gestor de usuarios</param>
        /// <param name="applicationDbContext">Contexto de la aplicación</param>
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext applicationDbContext)
        {
            try
            {
                // Verificar si ya existen usuarios en el sistema
                var existingUsers = await userManager.Users.CountAsync();
                if (existingUsers > 0)
                {
                    Console.WriteLine($"Ya existen {existingUsers} usuarios en el sistema. Saltando creación de usuarios de prueba.");
                    return;
                }

                Console.WriteLine("No existen usuarios en el sistema. Creando usuarios de prueba...");

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
                var supplierUser = await CreateUserAsync(userManager, 
                    "supplier", 
                    "supplier@spgsystem.com", 
                    "Empresa ABC", 
                    "Proveedor", 
                    "8095550004", 
                    "Supplier123!", 
                    Roles.Supplier);
                
                // Crear automáticamente la entidad Supplier para el usuario supplier
                if (supplierUser != null)
                {
                    await CreateSupplierEntityAsync(applicationDbContext, supplierUser);
                }

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
        private static async Task<ApplicationUser?> CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string userName,
            string email,
            string companyName,
            string contactName,
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
                    CompanyName = companyName,
                    ContactName = contactName,
                    PhoneNumber = phoneNumber,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, password);
                if (result.Succeeded)
                {
                    var roleResult = await userManager.AddToRoleAsync(newUser, role.ToString());
                    if (roleResult.Succeeded)
                    {
                        Console.WriteLine($"Usuario '{userName}' creado exitosamente con rol '{role}'.");
                        Console.WriteLine($"Usuario '{userName}' - Email: {email}, CompanyName: {companyName}, ContactName: {contactName}");
                        return newUser;
                    }
                    else
                    {
                        Console.WriteLine($"Error asignando rol '{role}' al usuario '{userName}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine($"Error creando usuario '{userName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                
                return null;
            }
            else
            {
                Console.WriteLine($"El usuario '{userName}' ya existe.");
                return user;
            }
        }
        
        /// <summary>
        /// Método auxiliar para crear la entidad Supplier para un usuario
        /// </summary>
        private static async Task CreateSupplierEntityAsync(ApplicationDbContext context, ApplicationUser user)
        {
            try
            {
                // Verificar si ya existe un proveedor para este usuario
                var existingSupplier = await context.Suppliers
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (existingSupplier == null)
                {
                    var supplier = new Supplier
                    {
                        Name = user.CompanyName ?? $"Empresa de {user.UserName}",
                        ContactPerson = user.ContactName ?? "Proveedor",
                        Email = user.Email,
                        Phone = user.PhoneNumber,
                        Address = "Dirección por defecto",
                        City = "Ciudad por defecto",
                        Country = "República Dominicana",
                        PostalCode = "00000",
                        TaxId = "00000000000",
                        IsActive = true,
                        UserId = user.Id, // Usar el ID real del usuario
                        CreatedDate = DateTime.Now
                    };

                    await context.Suppliers.AddAsync(supplier);
                    await context.SaveChangesAsync();
                    
                    Console.WriteLine($"Entidad Supplier creada exitosamente para usuario '{user.UserName}' (ID: {user.Id})");
                    Console.WriteLine($"Supplier - Nombre: {supplier.Name}, ContactPerson: {supplier.ContactPerson}");
                }
                else
                {
                    Console.WriteLine($"Ya existe una entidad Supplier para el usuario '{user.UserName}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando entidad Supplier para usuario '{user.UserName}': {ex.Message}");
            }
        }
    }
} 