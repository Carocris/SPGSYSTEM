using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.ViewModels.Product;
using AutoMapper;
using Database.Entities;
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
                var product = _mapper.Map<Product>(vm);
                await _productRepository.AddAsync(product);
                await _productRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ProductSaveViewModel vm)
        {
            try
            {
                var product = _mapper.Map<Product>(vm);
                _productRepository.Update(product);
                await _productRepository.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
