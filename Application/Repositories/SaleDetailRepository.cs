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
    public class SaleDetailRepository : GenericRepository<SaleDetail>, ISaleDetailRepository
    {
        public SaleDetailRepository(ApplicationDbContext db) : base(db) { }

        public async Task<SaleDetail> GetWithDetailsAsync(int id)
        {
            return await _db.SaleDetails
                            .Include(sd => sd.Product)
                            .Include(sd => sd.Sale)
                                .ThenInclude(s => s.Customer)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(sd => sd.Id == id);
        }

        public async Task<IEnumerable<SaleDetail>> GetAllWithDetailsAsync()
        {
            return await _db.SaleDetails
                            .Include(sd => sd.Product)
                            .Include(sd => sd.Sale)
                                .ThenInclude(s => s.Customer)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<IEnumerable<SaleDetail>> GetBySaleIdAsync(int saleId)
        {
            return await _db.SaleDetails
                            .Include(sd => sd.Product)
                            .Include(sd => sd.Sale)
                                .ThenInclude(s => s.Customer)
                            .Where(sd => sd.SaleId == saleId)
                            .AsNoTracking()
                            .ToListAsync();
        }
    }

}
