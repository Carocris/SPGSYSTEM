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
                Console.WriteLine($"SupplierService.GetByUserIdAsync: Buscando proveedor para userId: {userId}");
                
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("SupplierService.GetByUserIdAsync: userId es null o vacío");
                    return null;
                }

                // Obtener todos los proveedores
                var suppliers = await _supplierRepository.GetAllAsync();
                Console.WriteLine($"SupplierService.GetByUserIdAsync: Total de proveedores encontrados: {suppliers.Count}");
                
                // Buscar el proveedor que tenga el UserId especificado
                var supplier = suppliers.FirstOrDefault(s => s.UserId == userId);
                Console.WriteLine($"SupplierService.GetByUserIdAsync: Proveedor encontrado: {(supplier != null ? $"ID: {supplier.Id}, Name: {supplier.Name}, UserId: {supplier.UserId}" : "null")}");
                
                return supplier;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SupplierService.GetByUserIdAsync: Error - {ex.Message}");
                Console.WriteLine($"SupplierService.GetByUserIdAsync: Stack trace - {ex.StackTrace}");
                return null;
            }
        }

        public async Task<Supplier?> GetByUserNameAsync(string userName)
        {
            try
            {
                Console.WriteLine($"SupplierService.GetByUserNameAsync: Buscando proveedor para userName: {userName}");
                
                if (string.IsNullOrEmpty(userName))
                {
                    Console.WriteLine("SupplierService.GetByUserNameAsync: userName es null o vacío");
                    return null;
                }

                // Obtener todos los proveedores
                var suppliers = await _supplierRepository.GetAllAsync();
                Console.WriteLine($"SupplierService.GetByUserNameAsync: Total de proveedores encontrados: {suppliers.Count}");
                
                // Buscar el proveedor que tenga el UserId que corresponde al userName del sistema de autenticación
                // El userName del sistema de autenticación es el mismo que el UserId en la entidad Supplier
                var supplier = suppliers.FirstOrDefault(s => s.UserId == userName);
                Console.WriteLine($"SupplierService.GetByUserNameAsync: Proveedor encontrado: {(supplier != null ? $"ID: {supplier.Id}, Name: {supplier.Name}, UserId: {supplier.UserId}" : "null")}");
                
                return supplier;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SupplierService.GetByUserNameAsync: Error - {ex.Message}");
                Console.WriteLine($"SupplierService.GetByUserNameAsync: Stack trace - {ex.StackTrace}");
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
                Console.WriteLine($"SupplierService.CreateAsync: Creando proveedor {vm.Name} con UserId: {vm.UserId}");
                var supplier = _mapper.Map<Supplier>(vm);
                Console.WriteLine($"SupplierService.CreateAsync: Mapeo completado. Supplier.Name: {supplier.Name}, Supplier.UserId: {supplier.UserId}");
                
                await _supplierRepository.AddAsync(supplier);
                await _supplierRepository.SaveChangesAsync();
                
                Console.WriteLine($"SupplierService.CreateAsync: Proveedor {vm.Name} creado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SupplierService.CreateAsync: Error creando proveedor {vm.Name}: {ex.Message}");
                Console.WriteLine($"SupplierService.CreateAsync: Stack trace: {ex.StackTrace}");
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

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            try
            {
                Console.WriteLine($"SupplierService.DeleteSupplierAsync: Intentando eliminar proveedor con ID: {id}");
                
                var supplier = await _supplierRepository.GetByIdAsync(id);
                if (supplier == null)
                {
                    Console.WriteLine($"SupplierService.DeleteSupplierAsync: Proveedor con ID {id} no encontrado");
                    return false;
                }

                Console.WriteLine($"SupplierService.DeleteSupplierAsync: Proveedor encontrado - Nombre: {supplier.Name}");
                
                // Verificar si el proveedor tiene productos asociados
                try
                {
                    var products = await _supplierRepository.GetProductsBySupplierIdAsync(id);
                    Console.WriteLine($"SupplierService.DeleteSupplierAsync: Productos encontrados para el proveedor: {products.Count}");
                    
                    if (products.Any())
                    {
                        Console.WriteLine($"SupplierService.DeleteSupplierAsync: El proveedor tiene {products.Count} productos asociados. No se puede eliminar.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SupplierService.DeleteSupplierAsync: Error verificando productos: {ex.Message}");
                    return false;
                }

                Console.WriteLine($"SupplierService.DeleteSupplierAsync: El proveedor no tiene productos asociados. Procediendo a eliminar.");
                
                _supplierRepository.Delete(supplier);
                await _supplierRepository.SaveChangesAsync();
                
                Console.WriteLine($"SupplierService.DeleteSupplierAsync: Proveedor {supplier.Name} eliminado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SupplierService.DeleteSupplierAsync: Error eliminando proveedor con ID {id}: {ex.Message}");
                Console.WriteLine($"SupplierService.DeleteSupplierAsync: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        // Métodos adicionales para SuppliersController
        public async Task<List<Supplier>> GetAllWithProductsAsync()
        {
            try
            {
                // Obtener todos los proveedores con sus productos relacionados
                var suppliers = await _supplierRepository.GetAllAsync();
                
                // Para cada proveedor, cargar sus productos
                foreach (var supplier in suppliers)
                {
                    // Cargar los productos del proveedor
                    var products = await _supplierRepository.GetProductsBySupplierIdAsync(supplier.Id);
                    supplier.Products = products.ToList();
                }
                
                return suppliers.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SupplierService.GetAllWithProductsAsync: Error cargando proveedores con productos: {ex.Message}");
                return new List<Supplier>();
            }
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