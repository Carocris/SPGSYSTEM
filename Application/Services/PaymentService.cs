using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Database.Entities;
using Database.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PaymentService : GenericService<Payment>, IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
            : base(paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<Payment> GetBySaleIdAsync(int saleId)
        {
            return await _paymentRepository.GetBySaleIdAsync(saleId);
        }

        public async Task<Payment> GetWithDetailsAsync(int id)
        {
            return await _paymentRepository.GetWithDetailsAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetAllWithDetailsAsync()
        {
            return await _paymentRepository.GetAllWithDetailsAsync();
        }

        public async Task<IEnumerable<Payment>> GetByPaymentMethodAsync(PaymentMethodType paymentMethod)
        {
            return await _paymentRepository.GetByPaymentMethodAsync(paymentMethod);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _paymentRepository.GetPaymentsByDateRangeAsync(startDate, endDate);
        }
    }
}
