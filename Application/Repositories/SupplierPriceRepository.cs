using Application.Interfaces.Repositories;
using Database.Contexts;
using Database.Entities;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class SupplierPriceRepository : GenericRepository<SupplierPrice>, ISupplierPriceRepository
    {
        public SupplierPriceRepository(ApplicationDbContext db) : base(db) { }

        public async Task<IReadOnlyList<SupplierPrice>> GetBySupplierAsync(int supplierId)
        {
            return await _db.SupplierPrices
                            .Include(sp => sp.Supplier)
                            .Include(sp => sp.Product)
                            .Where(sp => sp.SupplierId == supplierId && sp.IsActive)
                            .OrderBy(sp => sp.Product.Name)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<IReadOnlyList<SupplierPrice>> GetByProductAsync(int productId)
        {
            return await _db.SupplierPrices
                            .Include(sp => sp.Supplier)
                            .Include(sp => sp.Product)
                            .Where(sp => sp.ProductId == productId && sp.IsActive)
                            .OrderBy(sp => sp.Price)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<SupplierPrice?> GetBySupplierAndProductAsync(int supplierId, int productId)
        {
            return await _db.SupplierPrices
                            .Include(sp => sp.Supplier)
                            .Include(sp => sp.Product)
                            .Where(sp => sp.SupplierId == supplierId && sp.ProductId == productId && sp.IsActive)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<SupplierPrice>> GetActiveAsync()
        {
            return await _db.SupplierPrices
                            .Include(sp => sp.Supplier)
                            .Include(sp => sp.Product)
                            .Where(sp => sp.IsActive)
                            .OrderBy(sp => sp.Supplier.Name)
                            .ThenBy(sp => sp.Product.Name)
                            .AsNoTracking()
                            .ToListAsync();
        }
    }
} 