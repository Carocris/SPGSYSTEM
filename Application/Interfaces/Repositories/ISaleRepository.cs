using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface ISaleRepository : IGenericRepository<Sale>
    {
        Task<Sale> GetFullSaleAsync(int id);
        Task<IReadOnlyList<Sale>> GetAllWithDetailsAsync();
    }
}
