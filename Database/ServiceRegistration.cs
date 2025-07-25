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
            // Configure DbContext: In-Memory for testing, SQL Server otherwise
            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("InventoryDb"));
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(
                        configuration.GetConnectionString("DefaultConnection"),
                        sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                    )
                );
            }

         
        }
    }
}
