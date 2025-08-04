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
    }
}
