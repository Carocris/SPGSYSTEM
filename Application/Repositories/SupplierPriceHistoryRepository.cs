using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.ViewModels.SupplierPriceHistory;
using AutoMapper;
using Database.Contexts;
using Database.Entities;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class SupplierPriceHistoryRepository : GenericRepository<SupplierPriceHistory>, ISupplierPriceHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SupplierPriceHistoryRepository(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<SupplierPriceHistoryViewModel>> GetBySupplierAsync(int supplierId)
        {
            var histories = await _context.SupplierPriceHistories
                .Include(sph => sph.Supplier)
                .Include(sph => sph.Product)
                .Where(sph => sph.SupplierId == supplierId)
                .OrderByDescending(sph => sph.ChangeDate)
                .ToListAsync();

            return _mapper.Map<List<SupplierPriceHistoryViewModel>>(histories);
        }

        public async Task<List<SupplierPriceHistoryViewModel>> GetByProductAsync(int productId)
        {
            var histories = await _context.SupplierPriceHistories
                .Include(sph => sph.Supplier)
                .Include(sph => sph.Product)
                .Where(sph => sph.ProductId == productId)
                .OrderByDescending(sph => sph.ChangeDate)
                .ToListAsync();

            return _mapper.Map<List<SupplierPriceHistoryViewModel>>(histories);
        }

        public async Task<List<SupplierPriceHistoryViewModel>> GetBySupplierAndProductAsync(int supplierId, int productId)
        {
            var histories = await _context.SupplierPriceHistories
                .Include(sph => sph.Supplier)
                .Include(sph => sph.Product)
                .Where(sph => sph.SupplierId == supplierId && sph.ProductId == productId)
                .OrderByDescending(sph => sph.ChangeDate)
                .ToListAsync();

            return _mapper.Map<List<SupplierPriceHistoryViewModel>>(histories);
        }

        public async Task<List<SupplierPriceHistoryViewModel>> GetRecentAsync(int count = 10)
        {
            var histories = await _context.SupplierPriceHistories
                .Include(sph => sph.Supplier)
                .Include(sph => sph.Product)
                .OrderByDescending(sph => sph.ChangeDate)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<List<SupplierPriceHistoryViewModel>>(histories);
        }
    }
} 