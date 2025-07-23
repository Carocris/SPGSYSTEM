using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Database.Entities;

namespace Application.Services
{
    public class SupplierPriceService : GenericService<SupplierPrice>, ISupplierPriceService
    {
        private readonly ISupplierPriceRepository _supplierPriceRepository;

        public SupplierPriceService(ISupplierPriceRepository supplierPriceRepository)
            : base(supplierPriceRepository)
        {
            _supplierPriceRepository = supplierPriceRepository;
        }

        public async Task<IReadOnlyList<SupplierPrice>> GetBySupplierAsync(int supplierId)
        {
            return await _supplierPriceRepository.GetBySupplierAsync(supplierId);
        }

        public async Task<IReadOnlyList<SupplierPrice>> GetByProductAsync(int productId)
        {
            return await _supplierPriceRepository.GetByProductAsync(productId);
        }

        public async Task<SupplierPrice?> GetBySupplierAndProductAsync(int supplierId, int productId)
        {
            return await _supplierPriceRepository.GetBySupplierAndProductAsync(supplierId, productId);
        }

        public async Task<IReadOnlyList<SupplierPrice>> GetActiveAsync()
        {
            return await _supplierPriceRepository.GetActiveAsync();
        }
    }
} 