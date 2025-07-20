using Database.Entities;
using Database.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IPaymentService : IGenericService<Payment>
    {
        Task<Payment> GetBySaleIdAsync(int saleId);
        Task<Payment> GetWithDetailsAsync(int id);
        Task<IEnumerable<Payment>> GetAllWithDetailsAsync();
        Task<IEnumerable<Payment>> GetByPaymentMethodAsync(PaymentMethodType paymentMethod);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
