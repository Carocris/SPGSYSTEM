using Application.Interfaces.Services;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ICategoryService : IGenericService<Category>
    {
        Task<Category> GetWithProductsAsync(int id);
        Task<IReadOnlyList<Category>> GetActiveAsync();
        Task<IReadOnlyList<Category>> GetAllWithProductsAsync();
        Task<bool> ExistsAsync(string name, int? excludeId = null);
    }
} 