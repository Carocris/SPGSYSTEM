using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Database.Entities;
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
    }
}
