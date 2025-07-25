using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.ViewModels.SupplierPriceHistory;
using AutoMapper;
using Database.Entities;

namespace Application.Services
{
    public class SupplierPriceHistoryService : GenericService<SupplierPriceHistory>, ISupplierPriceHistoryService
    {
        private readonly ISupplierPriceHistoryRepository _supplierPriceHistoryRepository;
        private readonly IMapper _mapper;

        public SupplierPriceHistoryService(ISupplierPriceHistoryRepository repository, IMapper mapper) : base(repository)
        {
            _supplierPriceHistoryRepository = repository;
            _mapper = mapper;
        }

        public async Task<List<SupplierPriceHistoryViewModel>> GetBySupplierAsync(int supplierId)
        {
            var histories = await _supplierPriceHistoryRepository.GetBySupplierAsync(supplierId);
            return histories;
        }

        public async Task<List<SupplierPriceHistoryViewModel>> GetByProductAsync(int productId)
        {
            var histories = await _supplierPriceHistoryRepository.GetByProductAsync(productId);
            return histories;
        }

        public async Task<List<SupplierPriceHistoryViewModel>> GetBySupplierAndProductAsync(int supplierId, int productId)
        {
            var histories = await _supplierPriceHistoryRepository.GetBySupplierAndProductAsync(supplierId, productId);
            return histories;
        }

        public async Task<List<SupplierPriceHistoryViewModel>> GetRecentAsync(int count = 10)
        {
            var histories = await _supplierPriceHistoryRepository.GetRecentAsync(count);
            return histories;
        }

        public async Task CreateHistoryRecordAsync(int supplierId, int productId, decimal oldPrice, decimal newPrice, string? changedBy = null, string? notes = null, string? changeReason = null)
        {
            var history = new SupplierPriceHistory
            {
                SupplierId = supplierId,
                ProductId = productId,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                Currency = "USD",
                ChangeDate = DateTime.Now,
                ChangedBy = changedBy ?? "Sistema",
                Notes = notes,
                ChangeReason = changeReason ?? "Actualización de precio"
            };

            await _supplierPriceHistoryRepository.AddAsync(history);
        }
    }
} 