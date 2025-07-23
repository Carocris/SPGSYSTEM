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
    public class SupplierService : GenericService<Supplier>, ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository) : base(supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public async Task<Supplier> GetWithProductsAsync(int id)
        {
            return await _supplierRepository.GetWithProductsAsync(id);
        }

        public async Task<IReadOnlyList<Supplier>> GetActiveAsync()
        {
            return await _supplierRepository.GetActiveAsync();
        }

        public async Task<IReadOnlyList<Supplier>> GetAllWithProductsAsync()
        {
            return await _supplierRepository.GetAllWithProductsAsync();
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            return await _supplierRepository.ExistsAsync(name, excludeId);
        }
    }
} 