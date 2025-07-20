using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Database.Contexts;
using Database.Entities;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext db) : base(db) { }

        public async Task<Product> GetWithDetailsAsync(int id)
        {
            return await _db.Products
                            .Include(p => p.SaleDetails)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
