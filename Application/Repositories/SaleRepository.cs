// File: Database/Repositories/SaleRepository.cs

using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Database.Contexts;
using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class SaleRepository : GenericRepository<Sale>, ISaleRepository
    {
        public SaleRepository(ApplicationDbContext db)
            : base(db)
        {
        }

        public async Task<Sale> GetFullSaleAsync(int id)
        {
            return await _db.Sales
                            .Include(s => s.Details)
                                .ThenInclude(d => d.Product)
                            .Include(s => s.Payment)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
