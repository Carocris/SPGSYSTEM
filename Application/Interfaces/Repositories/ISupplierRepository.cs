using Application.Interfaces.Repositories;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        Task<Supplier> GetWithProductsAsync(int id);
        Task<IReadOnlyList<Supplier>> GetActiveAsync();
        Task<IReadOnlyList<Supplier>> GetAllWithProductsAsync();
        Task<bool> ExistsAsync(string name, int? excludeId = null);
    }
} 