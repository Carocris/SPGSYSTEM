using Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application;
using Application.Mappings;
using Identity;


var builder = WebApplication.CreateBuilder(args);

// 1) MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// 2) Registrar DbContext y repositorios (Database/ServiceRegistration)
builder.Services.AddDatabaseInfrastructure(builder.Configuration);

// 3) Registrar servicios de aplicación (Application/ServiceRegistration)
builder.Services.AddApplicationLayer();

// 4) Registrar servicios de Identity
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddAutoMapper(typeof(GeneralProfile));


var app = builder.Build();

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Configurar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Ejecutar seeds al iniciar la aplicación
await app.Services.RunSeedsAsync();

// Configuración de rutas con autenticación
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Ruta para acceso denegado
app.MapControllerRoute(
    name: "access-denied",
    pattern: "Account/AccessDenied",
    defaults: new { controller = "Account", action = "AccessDenied" }
);

app.Run();
