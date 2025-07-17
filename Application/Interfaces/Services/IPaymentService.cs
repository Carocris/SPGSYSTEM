using Database.Entities;
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
    }
}
