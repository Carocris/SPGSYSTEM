using Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Application;
using Application.Mappings;


var builder = WebApplication.CreateBuilder(args);

// 1) MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// 2) Registrar DbContext y repositorios (Database/ServiceRegistration)
builder.Services.AddDatabaseInfrastructure(builder.Configuration);

// 3) Registrar servicios de aplicación (Application/ServiceRegistration)
builder.Services.AddApplicationLayer();

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

// Si luego necesitas autenticación/autorización:
// app.UseAuthentication();
app.UseAuthorization();

// Configuración de rutas más específica
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Rutas específicas para evitar ambigüedad
app.MapControllerRoute(
    name: "suppliers",
    pattern: "Suppliers/{action=Index}/{id?}",
    defaults: new { controller = "Suppliers" }
);

app.MapControllerRoute(
    name: "categories",
    pattern: "Categories/{action=Index}/{id?}",
    defaults: new { controller = "Categories" }
);

app.Run();
