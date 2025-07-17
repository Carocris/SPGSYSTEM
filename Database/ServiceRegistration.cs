//using Database.Contexts;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Application.Interfaces.Repositories;
//using Infrastructure.Persistence.Repositories;

//namespace Infrastructure.Persistence
//{
//    public static class ServicesRegistration
//    {
//        public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
//        {
//            // DbContext configuration: InMemory for testing or SQL Server for production
//            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
//            {
//                services.AddDbContext<ApplicationDbContext>(options =>
//                    options.UseInMemoryDatabase("InventoryDb"));
//            }
//            else
//            {
//                services.AddDbContext<ApplicationDbContext>(options =>
//                    options.UseSqlServer(
//                        configuration.GetConnectionString("DefaultConnection"),
//                        sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
//                    )
//                );
//            }

//            // Generic repository registration
//            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

//            // Specific repositories
//            services.AddTransient<ICustomerRepository, CustomerRepository>();
//            services.AddTransient<IProductRepository, ProductRepository>();
//            services.AddTransient<ISaleRepository, SaleRepository>();
//            services.AddTransient<ISaleDetailRepository, SaleDetailRepository>();
//            services.AddTransient<IPaymentRepository, PaymentRepository>();
//        }
//    }
//}
