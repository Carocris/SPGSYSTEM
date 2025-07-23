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
    public class CategoryService : GenericService<Category>, ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository) : base(categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Category> GetWithProductsAsync(int id)
        {
            return await _categoryRepository.GetWithProductsAsync(id);
        }

        public async Task<IReadOnlyList<Category>> GetActiveAsync()
        {
            return await _categoryRepository.GetActiveAsync();
        }

        public async Task<IReadOnlyList<Category>> GetAllWithProductsAsync()
        {
            return await _categoryRepository.GetAllWithProductsAsync();
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            return await _categoryRepository.ExistsAsync(name, excludeId);
        }
    }
} 