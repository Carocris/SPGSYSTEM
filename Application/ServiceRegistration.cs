using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Database.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            // Generic service
            services.AddTransient(typeof(IGenericService<>), typeof(GenericService<>));

            // services
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<ISaleService, SaleService>();
            services.AddTransient<ISaleDetailService, SaleDetailService>();
            services.AddTransient<IPaymentService, PaymentService>();

            // Generic repository registration
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Specific repository registrations
            services.AddTransient<ICustomerRepository, CustomerRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<ISaleRepository, SaleRepository>();
            services.AddTransient<ISaleDetailRepository, SaleDetailRepository>();
            services.AddTransient<IPaymentRepository, PaymentRepository>();
        }
    }
}
