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
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<SupplierPrice> SupplierPrices { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Primary keys
            modelBuilder.Entity<Customer>().HasKey(c => c.Id);
            modelBuilder.Entity<Product>().HasKey(p => p.Id);
            modelBuilder.Entity<Category>().HasKey(c => c.Id);
            modelBuilder.Entity<Supplier>().HasKey(s => s.Id);
            modelBuilder.Entity<Sale>().HasKey(s => s.Id);
            modelBuilder.Entity<SaleDetail>().HasKey(sd => sd.Id);
            modelBuilder.Entity<Payment>().HasKey(p => p.Id);
            modelBuilder.Entity<SupplierPrice>().HasKey(sp => sp.Id);
            #endregion

            #region Relationships
            // Sale -> Customer
            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // SaleDetail -> Sale
            modelBuilder.Entity<SaleDetail>()
                .HasOne(sd => sd.Sale)
                .WithMany(s => s.Details)
                .HasForeignKey(sd => sd.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            // SaleDetail -> Product
            modelBuilder.Entity<SaleDetail>()
                .HasOne(sd => sd.Product)
                .WithMany(p => p.SaleDetails)
                .HasForeignKey(sd => sd.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment -> Sale
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Sale)
                .WithOne(s => s.Payment)
                .HasForeignKey<Payment>(p => p.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product -> Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Product -> Supplier
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            // SupplierPrice -> Supplier
            modelBuilder.Entity<SupplierPrice>()
                .HasOne(sp => sp.Supplier)
                .WithMany(s => s.SupplierPrices)
                .HasForeignKey(sp => sp.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            // SupplierPrice -> Product
            modelBuilder.Entity<SupplierPrice>()
                .HasOne(sp => sp.Product)
                .WithMany(p => p.SupplierPrices)
                .HasForeignKey(sp => sp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region Property configurations
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

            // Category
            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Category>()
                .Property(c => c.Description)
                .HasMaxLength(500);

            // Supplier
            modelBuilder.Entity<Supplier>()
                .Property(s => s.Name)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Supplier>()
                .Property(s => s.ContactPerson)
                .HasMaxLength(100);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.Phone)
                .HasMaxLength(20);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.Email)
                .HasMaxLength(100);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.Address)
                .HasMaxLength(200);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.City)
                .HasMaxLength(50);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.PostalCode)
                .HasMaxLength(20);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.Country)
                .HasMaxLength(50);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.TaxId)
                .HasMaxLength(20);

            // Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Code)
                .HasMaxLength(20)
                .IsRequired();
            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Product>()
                .Property(p => p.Description)
                .HasMaxLength(500);
            modelBuilder.Entity<Product>()
                .Property(p => p.PurchasePrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            modelBuilder.Entity<Product>()
                .Property(p => p.SalePrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            modelBuilder.Entity<Product>()
                .Property(p => p.Stock)
                .IsRequired();
            modelBuilder.Entity<Product>()
                .Property(p => p.MinimumStock)
                .IsRequired();

            // Unique constraints
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Code)
                .IsUnique();
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

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
                .Property(p => p.Amount)
                .HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentDate)
                .IsRequired();
            modelBuilder.Entity<Payment>()
                .Property(p => p.CardNumber)
                .HasMaxLength(19);
            modelBuilder.Entity<Payment>()
                .Property(p => p.CardHolderName)
                .HasMaxLength(100);
            modelBuilder.Entity<Payment>()
                .Property(p => p.CardExpiryDate)
                .HasMaxLength(7);
            modelBuilder.Entity<Payment>()
                .Property(p => p.CardCVV)
                .HasMaxLength(4);
            modelBuilder.Entity<Payment>()
                .Property(p => p.TransferReference)
                .HasMaxLength(200);
            modelBuilder.Entity<Payment>()
                .Property(p => p.BankAccount)
                .HasMaxLength(100);
            modelBuilder.Entity<Payment>()
                .Property(p => p.TransferReceiptPath)
                .HasMaxLength(255);

            // SupplierPrice
            modelBuilder.Entity<SupplierPrice>()
                .Property(sp => sp.Price)
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            modelBuilder.Entity<SupplierPrice>()
                .Property(sp => sp.Currency)
                .HasMaxLength(20);
            modelBuilder.Entity<SupplierPrice>()
                .Property(sp => sp.Notes)
                .HasMaxLength(500);

            // Unique constraint for SupplierPrice (one price per supplier-product combination)
            modelBuilder.Entity<SupplierPrice>()
                .HasIndex(sp => new { sp.SupplierId, sp.ProductId })
                .IsUnique();
            #endregion
        }
    }
}
