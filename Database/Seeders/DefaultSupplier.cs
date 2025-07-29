using Database.Entities;
using Database.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Database.Seeders
{
    /// Seeder para crear el proveedor por defecto asociado al usuario supplier
    public static class DefaultSupplier
    {
        /// Crea el proveedor por defecto asociado al usuario supplier
        /// <param name="context">Contexto de la base de datos</param>
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                // Verificar si ya existe un proveedor con el UserId del usuario supplier
                var existingSupplier = await context.Suppliers
                    .FirstOrDefaultAsync(s => s.UserId == "supplier");

                if (existingSupplier == null)
                {
                    var supplier = new Supplier
                    {
                        Name = "Empresa ABC",
                        ContactPerson = "Proveedor",
                        Email = "supplier@spgsystem.com",
                        Phone = "8095550004",
                        Address = "Calle Principal #123",
                        City = "Santo Domingo",
                        Country = "República Dominicana",
                        PostalCode = "10101",
                        TaxId = "12345678901",
                        IsActive = true,
                        UserId = "supplier", // ID del usuario supplier creado en DefaultUsers
                        CreatedDate = DateTime.Now
                    };

                    await context.Suppliers.AddAsync(supplier);
                    await context.SaveChangesAsync();
                    
                    Console.WriteLine("Proveedor 'Empresa ABC' creado exitosamente y asociado al usuario 'supplier'.");
                }
                else
                {
                    Console.WriteLine("El proveedor para el usuario 'supplier' ya existe.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en DefaultSupplier.SeedAsync: {ex.Message}");
            }
        }
    }
}