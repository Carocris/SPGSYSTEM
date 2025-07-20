using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ISaleService : IGenericService<Sale>
    {
        Task<Sale> GetFullSaleAsync(int id);
        Task<Sale> CreateSaleAsync(Sale sale);
    }

}
