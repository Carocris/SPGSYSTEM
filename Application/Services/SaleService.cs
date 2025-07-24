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
    public class SaleService : GenericService<Sale>, ISaleService
    {
        private readonly ISaleRepository _saleRepository;

        public SaleService(ISaleRepository saleRepository)
            : base(saleRepository)
        {
            _saleRepository = saleRepository;
        }

        public async Task<Sale> GetFullSaleAsync(int id)
        {
            return await _saleRepository.GetFullSaleAsync(id);
        }

        public async Task<Sale> CreateSaleAsync(Sale sale)
        {
            await _saleRepository.AddAsync(sale);
            await _saleRepository.SaveChangesAsync();
            return sale;
        }

        public async Task<IReadOnlyList<Sale>> GetAllWithDetailsAsync()
        {
            return await _saleRepository.GetAllWithDetailsAsync();
        }

        public async Task<IReadOnlyList<Sale>> GetSalesWithoutPaymentAsync()
        {
            return await _saleRepository.GetSalesWithoutPaymentAsync();
        }
    }
}
