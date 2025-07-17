using Database;                                
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 1)  MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// 2) Registrar DbContext y repositorios (Database/ServiceRegistration)
builder.Services.AddDatabaseInfrastructure(builder.Configuration);

// 3) Aquí podrías añadir más servicios de tu capa Application si tuvieras:
//    e.g. builder.Services.AddApplicationServices(builder.Configuration);

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

// Ruta por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
