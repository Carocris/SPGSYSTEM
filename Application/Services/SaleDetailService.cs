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
    public class SaleDetailService : GenericService<SaleDetail>, ISaleDetailService
    {
        private readonly ISaleDetailRepository _saleDetailRepository;

        public SaleDetailService(ISaleDetailRepository repository)
            : base(repository)
        {
            _saleDetailRepository = repository;
        }

        public async Task<SaleDetail> GetWithDetailsAsync(int id)
        {
            return await _saleDetailRepository.GetWithDetailsAsync(id);
        }

        public async Task<IEnumerable<SaleDetail>> GetAllWithDetailsAsync()
        {
            return await _saleDetailRepository.GetAllWithDetailsAsync();
        }

        public async Task<IEnumerable<SaleDetail>> GetBySaleIdAsync(int saleId)
        {
            return await _saleDetailRepository.GetBySaleIdAsync(saleId);
        }
    }
}
