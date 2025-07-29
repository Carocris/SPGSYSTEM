using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.ViewModels.Supplier;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public SupplierService(ISupplierRepository supplierRepository, IMapper mapper) : base(supplierRepository)
        {
            _supplierRepository = supplierRepository;
            _mapper = mapper;
        }

        public async Task<Supplier?> GetByUserIdAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                    return null;

                // Obtener todos los proveedores
                var suppliers = await _supplierRepository.GetAllAsync();
                
                // Buscar el proveedor que tenga el UserId especificado
                return suppliers.FirstOrDefault(s => s.UserId == userId);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<SupplierViewModel>> GetAllViewModelsAsync()
        {
            var suppliers = await _supplierRepository.GetAllAsync();
            return _mapper.Map<List<SupplierViewModel>>(suppliers);
        }

        public async Task<SupplierViewModel?> GetViewModelByIdAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            return _mapper.Map<SupplierViewModel>(supplier);
        }

        public async Task<SupplierSaveViewModel?> GetSaveViewModelByIdAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            return _mapper.Map<SupplierSaveViewModel>(supplier);
        }

        public async Task<bool> CreateAsync(SupplierSaveViewModel vm)
        {
            try
            {
                var supplier = _mapper.Map<Supplier>(vm);
                await _supplierRepository.AddAsync(supplier);
                await _supplierRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(SupplierSaveViewModel vm)
        {
            try
            {
                var supplier = _mapper.Map<Supplier>(vm);
                _supplierRepository.Update(supplier);
                await _supplierRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Métodos adicionales para SuppliersController
        public async Task<List<Supplier>> GetAllWithProductsAsync()
        {
            var suppliers = await _supplierRepository.GetAllAsync();
            // Por ahora retornamos todos los proveedores
            // En una implementación real, cargaríamos los productos relacionados
            return suppliers.ToList();
        }

        public async Task<Supplier> GetWithProductsAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null)
                throw new InvalidOperationException("Proveedor no encontrado");
            
            // Por ahora retornamos el proveedor sin productos relacionados
            // En una implementación real, cargaríamos los productos
            return supplier;
        }

        public async Task<List<Supplier>> GetActiveAsync()
        {
            var suppliers = await _supplierRepository.GetAllAsync();
            // Por ahora retornamos todos los proveedores
            // En una implementación real, filtraríamos por estado activo
            return suppliers.ToList();
        }

        public async Task<bool> ExistsAsync(string name, int? excludeId = null)
        {
            var suppliers = await _supplierRepository.GetAllAsync();
            return suppliers.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && 
                                   (!excludeId.HasValue || s.Id != excludeId.Value));
        }
    }
} 