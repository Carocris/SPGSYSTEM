using Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Contexts
{
    /// <summary>
    /// Contexto de base de datos para la autenticación e identidad del sistema SPGSYSTEM
    /// </summary>
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        public IdentityContext(DbContextOptions<IdentityContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar el esquema por defecto para Identity
            modelBuilder.HasDefaultSchema("Identity");

            // Configurar la tabla de usuarios
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "Users");
                
                // Configurar propiedades adicionales del ApplicationUser
                            entity.Property(e => e.CompanyName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ContactName)
                .IsRequired()
                .HasMaxLength(100);
            });

            // Configurar la tabla de roles
            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Roles");
            });

            // Configurar la tabla de roles de usuario
            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable(name: "UserRoles");
            });

            // Configurar la tabla de logins de usuario
            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable(name: "UserLogins");
            });

            // Configurar la tabla de claims de usuario
            modelBuilder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable(name: "UserClaims");
            });

            // Configurar la tabla de claims de roles
            modelBuilder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable(name: "RoleClaims");
            });

            // Configurar la tabla de tokens de usuario
            modelBuilder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable(name: "UserTokens");
            });
        }
    }
}
