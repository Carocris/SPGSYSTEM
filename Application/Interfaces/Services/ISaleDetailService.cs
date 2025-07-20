using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ISaleDetailService : IGenericService<SaleDetail>
    {
        Task<SaleDetail> GetWithDetailsAsync(int id);
        Task<IEnumerable<SaleDetail>> GetAllWithDetailsAsync();
        Task<IEnumerable<SaleDetail>> GetBySaleIdAsync(int saleId);
    }
}
