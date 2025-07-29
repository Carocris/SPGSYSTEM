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
        public DbSet<SupplierPriceHistory> SupplierPriceHistories { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

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
            modelBuilder.Entity<SupplierPriceHistory>().HasKey(sph => sph.Id);
            modelBuilder.Entity<PurchaseOrder>().HasKey(po => po.Id);
            modelBuilder.Entity<PurchaseOrderDetail>().HasKey(pod => pod.Id);
            modelBuilder.Entity<Notification>().HasKey(n => n.Id);
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

            // SupplierPriceHistory -> Supplier
            modelBuilder.Entity<SupplierPriceHistory>()
                .HasOne(sph => sph.Supplier)
                .WithMany()
                .HasForeignKey(sph => sph.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);

            // SupplierPriceHistory -> Product
            modelBuilder.Entity<SupplierPriceHistory>()
                .HasOne(sph => sph.Product)
                .WithMany()
                .HasForeignKey(sph => sph.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // PurchaseOrder -> Supplier
            modelBuilder.Entity<PurchaseOrder>()
                .HasOne(po => po.Supplier)
                .WithMany()
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // PurchaseOrderDetail -> PurchaseOrder
            modelBuilder.Entity<PurchaseOrderDetail>()
                .HasOne(pod => pod.PurchaseOrder)
                .WithMany(po => po.Details)
                .HasForeignKey(pod => pod.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // PurchaseOrderDetail -> Product
            modelBuilder.Entity<PurchaseOrderDetail>()
                .HasOne(pod => pod.Product)
                .WithMany()
                .HasForeignKey(pod => pod.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification -> Supplier
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Supplier)
                .WithMany()
                .HasForeignKey(n => n.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region Properties
            // Customer
            modelBuilder.Entity<Customer>()
                .Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Customer>()
                .Property(c => c.Email)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Customer>()
                .Property(c => c.Phone)
                .HasMaxLength(20);

            // Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Code)
                .HasMaxLength(50)
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
                .HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Product>()
                .Property(p => p.SalePrice)
                .HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Product>()
                .Property(p => p.Stock)
                .IsRequired();
            modelBuilder.Entity<Product>()
                .Property(p => p.MinimumStock)
                .IsRequired();

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
                .Property(s => s.Email)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<Supplier>()
                .Property(s => s.Phone)
                .HasMaxLength(20);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.Address)
                .HasMaxLength(200);

            // SupplierPriceHistory
            modelBuilder.Entity<SupplierPriceHistory>()
                .Property(sph => sph.OldPrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            modelBuilder.Entity<SupplierPriceHistory>()
                .Property(sph => sph.NewPrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired();
            modelBuilder.Entity<SupplierPriceHistory>()
                .Property(sph => sph.ChangeDate)
                .IsRequired();

            // PurchaseOrder
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.OrderNumber)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.OrderDate)
                .IsRequired();
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.TotalAmount)
                .HasColumnType("decimal(10,2)");
            modelBuilder.Entity<PurchaseOrder>()
                .Property(po => po.Status)
                .HasMaxLength(50)
                .IsRequired();

            // PurchaseOrderDetail
            modelBuilder.Entity<PurchaseOrderDetail>()
                .Property(pod => pod.Quantity)
                .IsRequired();
            modelBuilder.Entity<PurchaseOrderDetail>()
                .Property(pod => pod.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Notification
            modelBuilder.Entity<Notification>()
                .Property(n => n.Title)
                .HasMaxLength(200)
                .IsRequired();
            modelBuilder.Entity<Notification>()
                .Property(n => n.Message)
                .HasMaxLength(1000)
                .IsRequired();
            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .HasMaxLength(50)
                .IsRequired();
            modelBuilder.Entity<Notification>()
                .Property(n => n.ProductName)
                .HasMaxLength(200);
            modelBuilder.Entity<Notification>()
                .Property(n => n.CustomerName)
                .HasMaxLength(200);
            #endregion

            #region Unique constraints
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
                .Property(p => p.Status)
                .IsRequired();
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
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
            #endregion
        }
    }
}

