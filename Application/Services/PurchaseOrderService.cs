using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.ViewModels.PurchaseOrder;
using AutoMapper;
using Database.Entities;

namespace Application.Services
{
    public class PurchaseOrderService : GenericService<PurchaseOrder>, IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IMapper _mapper;

        public PurchaseOrderService(IPurchaseOrderRepository repository, IMapper mapper) : base(repository)
        {
            _purchaseOrderRepository = repository;
            _mapper = mapper;
        }

        public async Task<List<PurchaseOrderViewModel>> GetBySupplierAsync(int supplierId)
        {
            var orders = await _purchaseOrderRepository.GetBySupplierAsync(supplierId);
            return orders;
        }

        public async Task<List<PurchaseOrderViewModel>> GetByStatusAsync(string status)
        {
            var orders = await _purchaseOrderRepository.GetByStatusAsync(status);
            return orders;
        }

        public async Task<List<PurchaseOrderViewModel>> GetRecentAsync(int count = 10)
        {
            var orders = await _purchaseOrderRepository.GetRecentAsync(count);
            return orders;
        }

        public async Task<PurchaseOrderViewModel> GetByIdWithDetailsAsync(int id)
        {
            var order = await _purchaseOrderRepository.GetByIdWithDetailsAsync(id);
            return order;
        }

        public async Task<bool> UpdateStatusAsync(int id, string newStatus)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(id);
            if (order == null) return false;

            order.Status = newStatus;
            order.LastModified = DateTime.Now;
            _purchaseOrderRepository.Update(order);
            await _purchaseOrderRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateDeliveryDateAsync(int id, DateTime deliveryDate)
        {
            var order = await _purchaseOrderRepository.GetByIdAsync(id);
            if (order == null) return false;

            order.ActualDeliveryDate = deliveryDate;
            order.LastModified = DateTime.Now;
            _purchaseOrderRepository.Update(order);
            await _purchaseOrderRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateReceivedQuantityAsync(int detailId, int receivedQuantity)
        {
            // Esta implementación requeriría acceso al repositorio de PurchaseOrderDetail
            // Por simplicidad, retornamos false por ahora
            return false;
        }
    }
} 