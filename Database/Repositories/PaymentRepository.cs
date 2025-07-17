// File: Database/Repositories/PaymentRepository.cs

using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Database.Contexts;
using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext db)
            : base(db)
        {
        }

        public async Task<Payment> GetBySaleIdAsync(int saleId)
        {
            return await _db.Payments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.SaleId == saleId);
        }
    }
}
