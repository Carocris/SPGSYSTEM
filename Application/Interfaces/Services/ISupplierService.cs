using Application.Interfaces.Services;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ISupplierService : IGenericService<Supplier>
    {
        Task<Supplier> GetWithProductsAsync(int id);
        Task<IReadOnlyList<Supplier>> GetActiveAsync();
        Task<bool> ExistsAsync(string name, int? excludeId = null);
    }
} 