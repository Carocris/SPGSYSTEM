using Application.Interfaces.Repositories;
using Database.Contexts;
using Database.Entities;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(ApplicationDbContext db) : base(db) { }

        public async Task<Supplier> GetWithProductsAsync(int id)
        {
            return await _db.Suppliers
                            .Include(s => s.Products)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IReadOnlyList<Supplier>> GetActiveAsync()
        {
            return await _db.Suppliers
                            .Where(s => s.IsActive)
                            .OrderBy(s => s.Name)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            var query = _db.Suppliers.Where(s => s.Name.ToLower() == name.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }
    }
} 