using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Database.Contexts;

namespace Database
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            // CAMBIADO PARA USAR SQL SERVER EXPRESS EN LUGAR DE LOCALDB
            optionsBuilder.UseSqlServer("Server=DESKTOP-EU4BPTQ\\SQLEXPRESS;Database=SPGSYSTEM;Trusted_Connection=True;TrustServerCertificate=true;MultipleActiveResultSets=true;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
} 