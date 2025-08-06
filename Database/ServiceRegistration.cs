using Database.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Database
{
    public static class ServiceRegistration
    {
        public static void AddDatabaseInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure DbContext: SQL Server - USAR LA MISMA CONEXIÓN QUE IDENTITY
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"), // USAR DefaultConnection
                    sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                )
            );
        }

        public static async Task RunDatabaseSeedsAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var applicationDbContext = services.GetRequiredService<ApplicationDbContext>();

                    // No ejecutar seeders de productos - el usuario los creará manualmente
                    Console.WriteLine("Database seeds completados exitosamente.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error ejecutando database seeds: {ex.Message}");
                }
            }
        }
    }
}
