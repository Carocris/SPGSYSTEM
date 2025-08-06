using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.ViewModels.Product;
using AutoMapper;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProductService : GenericService<Product>, IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, IMapper mapper) : base(productRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<List<Product>> GetBySupplierAsync(int supplierId)
        {
            var allProducts = await _productRepository.GetAllAsync();
            return allProducts.Where(p => p.SupplierId == supplierId).ToList();
        }

        public async Task<List<ProductViewModel>> GetAllViewModelsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<List<ProductViewModel>>(products);
        }

        public async Task<ProductViewModel?> GetViewModelByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return _mapper.Map<ProductViewModel>(product);
        }

        public async Task<ProductSaveViewModel?> GetSaveViewModelByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return _mapper.Map<ProductSaveViewModel>(product);
        }

        public async Task<bool> CreateAsync(ProductSaveViewModel vm)
        {
            try
            {
                Console.WriteLine($"ProductService.CreateAsync: Iniciando creación de producto '{vm.Name}'");
                Console.WriteLine($"ProductService.CreateAsync: SupplierId: {vm.SupplierId}");
                
                // Verificar si ya existe un producto con el mismo nombre SOLO para el mismo proveedor
                var existingProducts = await _productRepository.GetAllAsync();
                if (existingProducts.Any(p => p.Name.Equals(vm.Name, StringComparison.OrdinalIgnoreCase) && 
                                            p.SupplierId == vm.SupplierId))
                {
                    Console.WriteLine($"ProductService.CreateAsync: Producto duplicado encontrado para el proveedor {vm.SupplierId}: {vm.Name}");
                    return false; // Producto duplicado para el mismo proveedor
                }

                var product = _mapper.Map<Product>(vm);
                Console.WriteLine($"ProductService.CreateAsync: Producto mapeado - ID: {product.Id}, Name: {product.Name}, SupplierId: {product.SupplierId}");
                
                await _productRepository.AddAsync(product);
                await _productRepository.SaveChangesAsync();
                
                Console.WriteLine($"ProductService.CreateAsync: Producto '{vm.Name}' creado exitosamente con ID: {product.Id}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProductService.CreateAsync: Error creando producto '{vm.Name}': {ex.Message}");
                Console.WriteLine($"ProductService.CreateAsync: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ProductSaveViewModel vm)
        {
            try
            {
                Console.WriteLine($"ProductService.UpdateAsync: Iniciando actualización de producto '{vm.Name}' con ID: {vm.Id}");
                
                // Verificar si ya existe otro producto con el mismo nombre SOLO para el mismo proveedor (excluyendo el actual)
                var existingProducts = await _productRepository.GetAllAsync();
                if (existingProducts.Any(p => p.Name.Equals(vm.Name, StringComparison.OrdinalIgnoreCase) && 
                                            p.SupplierId == vm.SupplierId && 
                                            p.Id != vm.Id))
                {
                    Console.WriteLine($"ProductService.UpdateAsync: Producto duplicado encontrado para el proveedor {vm.SupplierId}: {vm.Name}");
                    return false; // Producto duplicado para el mismo proveedor
                }

                // Obtener la entidad existente del contexto
                var existingProduct = await _productRepository.GetByIdAsync(vm.Id);
                if (existingProduct == null)
                {
                    Console.WriteLine($"ProductService.UpdateAsync: Producto con ID {vm.Id} no encontrado");
                    return false;
                }

                // Actualizar las propiedades de la entidad existente
                existingProduct.Name = vm.Name;
                existingProduct.Code = vm.Code;
                existingProduct.Description = vm.Description;
                existingProduct.CategoryId = vm.CategoryId;
                existingProduct.SupplierId = vm.SupplierId;
                existingProduct.PurchasePrice = vm.PurchasePrice;
                existingProduct.SalePrice = vm.SalePrice;
                existingProduct.Stock = vm.Stock;
                existingProduct.MinimumStock = vm.MinimumStock;
                existingProduct.IsActive = vm.IsActive;
                existingProduct.LastUpdated = DateTime.Now;

                Console.WriteLine($"ProductService.UpdateAsync: Producto actualizado - ID: {existingProduct.Id}, Name: {existingProduct.Name}");
                
                // Actualizar la entidad existente
                _productRepository.Update(existingProduct);
                await _productRepository.SaveChangesAsync();
                
                Console.WriteLine($"ProductService.UpdateAsync: Producto '{vm.Name}' actualizado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProductService.UpdateAsync: Error actualizando producto '{vm.Name}': {ex.Message}");
                Console.WriteLine($"ProductService.UpdateAsync: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, int? supplierId = null)
        {
            var products = await _productRepository.GetAllAsync();
            return products.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && 
                                   (!excludeId.HasValue || p.Id != excludeId.Value) &&
                                   (!supplierId.HasValue || p.SupplierId == supplierId.Value));
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantityToAdd, decimal? newPurchasePrice = null)
        {
            try
            {
                Console.WriteLine($"ProductService.UpdateStockAsync: Actualizando stock del producto ID {productId}");
                
                // Obtener la entidad existente del contexto
                var existingProduct = await _productRepository.GetByIdAsync(productId);
                if (existingProduct == null)
                {
                    Console.WriteLine($"ProductService.UpdateStockAsync: Producto con ID {productId} no encontrado");
                    return false;
                }

                // Actualizar stock
                existingProduct.Stock += quantityToAdd;
                
                // Actualizar precio de compra si se especificó
                if (newPurchasePrice.HasValue && newPurchasePrice.Value > 0)
                {
                    existingProduct.PurchasePrice = newPurchasePrice.Value;
                }
                
                existingProduct.LastUpdated = DateTime.Now;

                Console.WriteLine($"ProductService.UpdateStockAsync: Stock actualizado - ID: {existingProduct.Id}, Stock: {existingProduct.Stock}");
                
                // Actualizar la entidad existente
                _productRepository.Update(existingProduct);
                await _productRepository.SaveChangesAsync();
                
                Console.WriteLine($"ProductService.UpdateStockAsync: Stock del producto actualizado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProductService.UpdateStockAsync: Error actualizando stock del producto ID {productId}: {ex.Message}");
                Console.WriteLine($"ProductService.UpdateStockAsync: Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
