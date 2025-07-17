using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ICustomerService : IGenericService<Customer>
    {
        Task<Customer> GetWithSalesAsync(int id);
    }
}
