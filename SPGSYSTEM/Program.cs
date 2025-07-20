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

// 3) Registrar servicios de aplicaci�n (Application/ServiceRegistration)
builder.Services.AddApplicationLayer();

builder.Services.AddAutoMapper(typeof(GeneralProfile));


var app = builder.Build();

// Configuraci�n del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Si luego necesitas autenticaci�n/autorizaci�n:
// app.UseAuthentication();
app.UseAuthorization();

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
