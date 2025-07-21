using Application.Interfaces.Repositories;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category> GetWithProductsAsync(int id);
        Task<IReadOnlyList<Category>> GetActiveAsync();
        Task<bool> ExistsAsync(string name, int? excludeId = null);
    }
} 