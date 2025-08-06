using Application.ViewModels.Product;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IProductService : IGenericService<Product>
    {
        Task<List<Product>> GetBySupplierAsync(int supplierId);
        Task<List<ProductViewModel>> GetAllViewModelsAsync();
        Task<ProductViewModel?> GetViewModelByIdAsync(int id);
        Task<ProductSaveViewModel?> GetSaveViewModelByIdAsync(int id);
        Task<bool> CreateAsync(ProductSaveViewModel vm);
        Task<bool> UpdateAsync(ProductSaveViewModel vm);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null, int? supplierId = null);
        Task<bool> UpdateStockAsync(int productId, int quantityToAdd, decimal? newPurchasePrice = null);
    }
}
