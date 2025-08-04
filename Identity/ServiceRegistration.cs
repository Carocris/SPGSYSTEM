using Identity.Contexts;
using Identity.Entities;
using Identity.Services;
using Identity.Interfaces;
using Database.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity
{
    /// <summary>
    /// Configuración de servicios para el proyecto Identity
    /// </summary>
    public static class ServiceRegistration
    {
        /// <summary>
        /// Registra los servicios de Identity en el contenedor de dependencias
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración de la aplicación</param>
        public static void AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            #region Contexts

            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<IdentityContext>(options => options.UseInMemoryDatabase("IdentityDbs"));
            }
            else
            {
                services.AddDbContext<IdentityContext>(opt =>
                {
                    opt.EnableSensitiveDataLogging();
                    opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                        m => m.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));
                });
            }
            #endregion

            #region Identity

            services.AddIdentityCore<ApplicationUser>(options =>
            {
                // Configurar opciones de validación
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = false;
                
                // Configurar opciones de contraseña
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
            })
                    .AddRoles<IdentityRole>()
                    .AddSignInManager()
                    .AddEntityFrameworkStores<IdentityContext>()
                    .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider);

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromSeconds(300);
            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            }).AddCookie(IdentityConstants.ApplicationScheme, opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromHours(24);
                opt.LoginPath = "/Account/Login";
                opt.AccessDeniedPath = "/Account/AccessDenied";
            });
            #endregion

            #region Services
            services.AddTransient<IAccountService, AccountService>();
            #endregion
        }

        #region Seeds
        public static async Task RunSeedsAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var applicationDbContext = services.GetRequiredService<Database.Contexts.ApplicationDbContext>();

                    await SeedData.SeedAsync(userManager, roleManager, applicationDbContext);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error ejecutando seeds: {ex.Message}");
                }
            }
        }
        #endregion
    }
} 