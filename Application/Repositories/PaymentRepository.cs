// File: Database/Repositories/PaymentRepository.cs

using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Database.Contexts;
using Database.Entities;
using Database.Enum;
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

        public async Task<Payment> GetWithDetailsAsync(int id)
        {
            return await _db.Payments
                            .Include(p => p.Sale)
                                .ThenInclude(s => s.Customer)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payment>> GetAllWithDetailsAsync()
        {
            return await _db.Payments
                            .Include(p => p.Sale)
                                .ThenInclude(s => s.Customer)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByPaymentMethodAsync(PaymentMethodType paymentMethod)
        {
            return await _db.Payments
                            .Include(p => p.Sale)
                                .ThenInclude(s => s.Customer)
                            .Where(p => p.PaymentMethod == paymentMethod)
                            .AsNoTracking()
                            .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _db.Payments
                            .Include(p => p.Sale)
                                .ThenInclude(s => s.Customer)
                            .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                            .AsNoTracking()
                            .ToListAsync();
        }
    }
}
