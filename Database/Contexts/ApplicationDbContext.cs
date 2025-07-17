using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options){}

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {


            #region            // Primary keys

            modelBuilder.Entity<Customer>().HasKey(c => c.Id);
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Sale>().HasKey(s => s.Id);
            modelBuilder.Entity<SaleDetail>().HasKey(sd => sd.Id);
            modelBuilder.Entity<Payment>().HasKey(p => p.Id);

            #endregion

            #region// Relationships
            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleDetail>()
                .HasOne(sd => sd.Sale)
                .WithMany(s => s.Details)
                .HasForeignKey(sd => sd.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaleDetail>()
                .HasOne(sd => sd.Product)
                .WithMany(p => p.SaleDetails)
                .HasForeignKey(sd => sd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Sale)
                .WithOne(s => s.Payment)
                .HasForeignKey<Payment>(p => p.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            #endregion

            #region // Property configurations
            // Customer
            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Customer>()
                .Property(c => c.Email)
                .HasMaxLength(100);
            modelBuilder.Entity<Customer>()
                .Property(c => c.Phone)
                .HasMaxLength(20);

            // Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Product>()
                .Property(p => p.Stock)
                .IsRequired();

            // Sale
            modelBuilder.Entity<Sale>()
                .Property(s => s.SaleDate)
                .IsRequired();
            modelBuilder.Entity<Sale>()
                .Property(s => s.TotalAmount)
                .HasColumnType("decimal(10,2)");

            // SaleDetail
            modelBuilder.Entity<SaleDetail>()
                .Property(sd => sd.Quantity)
                .IsRequired();
            modelBuilder.Entity<SaleDetail>()
                .Property(sd => sd.Subtotal)
                .HasColumnType("decimal(10,2)");

            // Payment
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .IsRequired();
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentDate)
                .IsRequired();
            #endregion

            base.OnModelCreating(modelBuilder);
        }

    }
}
