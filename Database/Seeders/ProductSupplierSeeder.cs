using Database.Entities;
using Database.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Database.Seeders
{
    /// <summary>
    /// Seeder para asignar productos a proveedores
    /// </summary>
    public static class ProductSupplierSeeder
    {
        /// <summary>
        /// Asigna productos existentes a proveedores
        /// </summary>
        /// <param name="context">Contexto de la base de datos</param>
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                // Obtener el proveedor por defecto
                var defaultSupplier = await context.Suppliers
                    .FirstOrDefaultAsync(s => s.UserId == "supplier");

                if (defaultSupplier == null)
                {
                    Console.WriteLine("No se encontró el proveedor por defecto. Ejecute DefaultSupplier primero.");
                    return;
                }

                // Obtener productos que no tienen SupplierId asignado
                var productsWithoutSupplier = await context.Products
                    .Where(p => p.SupplierId == null)
                    .ToListAsync();

                if (productsWithoutSupplier.Any())
                {
                    Console.WriteLine($"Encontrados {productsWithoutSupplier.Count} productos sin proveedor asignado.");

                    foreach (var product in productsWithoutSupplier)
                    {
                        product.SupplierId = defaultSupplier.Id;
                        Console.WriteLine($"Asignando producto '{product.Name}' al proveedor '{defaultSupplier.Name}'");
                    }

                    await context.SaveChangesAsync();
                    Console.WriteLine($"Se asignaron {productsWithoutSupplier.Count} productos al proveedor por defecto.");
                }
                else
                {
                    Console.WriteLine("Todos los productos ya tienen proveedor asignado.");
                }

                // Mostrar estadísticas
                var totalProducts = await context.Products.CountAsync();
                var productsWithSupplier = await context.Products.CountAsync(p => p.SupplierId != null);
                var productsWithoutSupplierCount = await context.Products.CountAsync(p => p.SupplierId == null);

                Console.WriteLine($"Estadísticas de productos:");
                Console.WriteLine($"- Total de productos: {totalProducts}");
                Console.WriteLine($"- Productos con proveedor: {productsWithSupplier}");
                Console.WriteLine($"- Productos sin proveedor: {productsWithoutSupplierCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en ProductSupplierSeeder.SeedAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
} 