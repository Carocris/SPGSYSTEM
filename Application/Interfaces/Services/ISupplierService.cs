using Application.Interfaces.Services;
using Application.ViewModels.Supplier;
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
        Task<Supplier?> GetByUserIdAsync(string userId);
        Task<List<SupplierViewModel>> GetAllViewModelsAsync();
        Task<SupplierViewModel?> GetViewModelByIdAsync(int id);
        Task<SupplierSaveViewModel?> GetSaveViewModelByIdAsync(int id);
        Task<bool> CreateAsync(SupplierSaveViewModel vm);
        Task<bool> UpdateAsync(SupplierSaveViewModel vm);
        
        // Métodos adicionales para SuppliersController
        Task<List<Supplier>> GetAllWithProductsAsync();
        Task<Supplier> GetWithProductsAsync(int id);
        Task<List<Supplier>> GetActiveAsync();
        Task<bool> ExistsAsync(string name, int? excludeId = null);
    }
} 