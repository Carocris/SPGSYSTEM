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
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext db) : base(db) { }

        public async Task<Category> GetWithProductsAsync(int id)
        {
            return await _db.Categories
                            .Include(c => c.Products)
                                .ThenInclude(p => p.Supplier)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IReadOnlyList<Category>> GetActiveAsync()
        {
            return await _db.Categories
                            .Include(c => c.Products)
                                .ThenInclude(p => p.Supplier)
                            .Where(c => c.IsActive)
                            .OrderBy(c => c.Name)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<IReadOnlyList<Category>> GetAllWithProductsAsync()
        {
            return await _db.Categories
                            .Include(c => c.Products)
                                .ThenInclude(p => p.Supplier)
                            .OrderBy(c => c.Name)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            var query = _db.Categories.Where(c => c.Name.ToLower() == name.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }

        public async Task<IReadOnlyList<Category>> GetCategoriesBySupplierUserIdAsync(string userId)
        {
            return await _db.Categories
                            .Include(c => c.Products)
                                .ThenInclude(p => p.Supplier)
                            .Include(c => c.Supplier)
                            .Where(c => c.Supplier.UserId == userId)
                            .OrderBy(c => c.Name)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<Category> CreateCategoryForSupplierAsync(Category category, string userId)
        {
            // Buscar el proveedor por userId
            var supplier = await _db.Suppliers
                                   .FirstOrDefaultAsync(s => s.UserId == userId);
            
            if (supplier == null)
            {
                throw new InvalidOperationException("Proveedor no encontrado para el usuario especificado.");
            }

            // Asignar el proveedor a la categoría
            category.SupplierId = supplier.Id;
            category.Supplier = supplier;

            // Guardar la categoría
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();

            return category;
        }
    }
} 