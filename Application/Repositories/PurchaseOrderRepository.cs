using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.ViewModels.PurchaseOrder;
using AutoMapper;
using Database.Contexts;
using Database.Entities;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class PurchaseOrderRepository : GenericRepository<PurchaseOrder>, IPurchaseOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PurchaseOrderRepository(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<PurchaseOrderViewModel>> GetBySupplierAsync(int supplierId)
        {
            var orders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Where(po => po.SupplierId == supplierId)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();

            return _mapper.Map<List<PurchaseOrderViewModel>>(orders);
        }

        public async Task<List<PurchaseOrderViewModel>> GetByStatusAsync(string status)
        {
            var orders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .Where(po => po.Status == status)
                .OrderByDescending(po => po.OrderDate)
                .ToListAsync();

            return _mapper.Map<List<PurchaseOrderViewModel>>(orders);
        }

        public async Task<List<PurchaseOrderViewModel>> GetRecentAsync(int count = 10)
        {
            var orders = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .OrderByDescending(po => po.OrderDate)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<List<PurchaseOrderViewModel>>(orders);
        }

        public async Task<PurchaseOrderViewModel> GetByIdWithDetailsAsync(int id)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(po => po.Id == id);

            return _mapper.Map<PurchaseOrderViewModel>(order);
        }
    }
} 