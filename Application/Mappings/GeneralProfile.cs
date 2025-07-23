using Application.ViewModels.Customer;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Application.ViewModels.Product;
using Application.ViewModels.SaleDetail;
using Application.ViewModels.Sale;
using Application.ViewModels.Payment;
using Application.ViewModels.Category;
using Application.ViewModels.Supplier;
using Application.ViewModels.SupplierPrice;

namespace Application.Mappings
{
    public class GeneralProfile : Profile
    {

        public GeneralProfile() {

            // Customer
            CreateMap<Customer, CustomerViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.Sales, opt => opt.Ignore());

            CreateMap<CustomerSaveViewModel, Customer>()
                .ForMember(dest => dest.Sales, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ReverseMap();

            // Category
            CreateMap<Category, CategoryViewModel>()
                .ForMember(dest => dest.ProductCount, 
                           opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0))
                .ReverseMap();

            CreateMap<CategorySaveViewModel, Category>()
                .ReverseMap();

            // Supplier
            CreateMap<Supplier, SupplierViewModel>()
                .ForMember(dest => dest.ProductCount, 
                           opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0))
                .ReverseMap();

            CreateMap<SupplierSaveViewModel, Supplier>()
                .ReverseMap();

            // Product
            CreateMap<Product, ProductViewModel>()
                .ForMember(dest => dest.CategoryName, 
                           opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
                .ForMember(dest => dest.SupplierName, 
                           opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
                .ForMember(dest => dest.ProfitMargin, 
                           opt => opt.MapFrom(src => src.ProfitMargin))
                .ForMember(dest => dest.IsLowStock, 
                           opt => opt.MapFrom(src => src.IsLowStock))
                .ForMember(dest => dest.InventoryValue, 
                           opt => opt.MapFrom(src => src.InventoryValue))
                .ReverseMap()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Supplier, opt => opt.Ignore())
                .ForMember(dest => dest.SaleDetails, opt => opt.Ignore());

            CreateMap<ProductSaveViewModel, Product>()
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Supplier, opt => opt.Ignore())
                .ForMember(dest => dest.SaleDetails, opt => opt.Ignore())
                .ForMember(dest => dest.SupplierPrices, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Price, opt => opt.Ignore()); // Evitar conflicto con SalePrice

            // SaleDetail
            CreateMap<SaleDetail, SaleDetailViewModel>()
                .ForMember(dest => dest.ProductName,
                           opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.SaleNumber,
                           opt => opt.MapFrom(src => src.Sale.Id.ToString("D4")))
                .ForMember(dest => dest.CustomerName,
                           opt => opt.MapFrom(src => src.Sale.Customer.Name))
                .ForMember(dest => dest.SaleDate,
                           opt => opt.MapFrom(src => src.Sale.SaleDate))
                .ForMember(dest => dest.UnitPrice,
                           opt => opt.MapFrom(src => src.Product.Price));

            CreateMap<SaleDetailSaveViewModel, SaleDetail>()
                .ReverseMap();

            // Sale
            CreateMap<Sale, SaleViewModel>()
                .ForMember(dest => dest.CustomerName,
                           opt => opt.MapFrom(src => src.Customer.Name))
                .ForMember(dest => dest.PaymentMethod,
                           opt => opt.MapFrom(src => src.Payment.PaymentMethod))
                .ForMember(dest => dest.Payment,
                           opt => opt.MapFrom(src => src.Payment))
                .ForMember(dest => dest.TotalItems,
                           opt => opt.MapFrom(src => src.Details != null ? src.Details.Sum(d => d.Quantity) : 0));
            CreateMap<SaleSaveViewModel, Sale>()
                .ForMember(dest => dest.SaleDate, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.Details, opt => opt.Ignore())
                .ForMember(dest => dest.Payment, opt => opt.Ignore());

            // Payment
            CreateMap<Payment, PaymentViewModel>()
                .ForMember(dest => dest.SaleNumber,
                           opt => opt.MapFrom(src => src.Sale.Id.ToString("D4")))
                .ForMember(dest => dest.CustomerName,
                           opt => opt.MapFrom(src => src.Sale.Customer.Name))
                .ForMember(dest => dest.SaleDate,
                           opt => opt.MapFrom(src => src.Sale.SaleDate))
                .ForMember(dest => dest.SaleTotal,
                           opt => opt.MapFrom(src => src.Sale.TotalAmount))
                .ForMember(dest => dest.PaymentStatus,
                           opt => opt.MapFrom(src => "Completado"));
                           
            CreateMap<PaymentSaveViewModel, Payment>()
                .ForMember(dest => dest.PaymentDate, opt => opt.Ignore())
                .ForMember(dest => dest.Sale, opt => opt.Ignore());

            // SupplierPrice
            CreateMap<SupplierPrice, SupplierPriceViewModel>()
                .ForMember(dest => dest.SupplierName, 
                           opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
                .ForMember(dest => dest.ProductName, 
                           opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.ProductCode, 
                           opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : null))
                .ReverseMap()
                .ForMember(dest => dest.Supplier, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            CreateMap<SupplierPriceSaveViewModel, SupplierPrice>()
                .ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => DateTime.Now))
                .ReverseMap()
                .ForMember(dest => dest.SupplierName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.ProductCode, opt => opt.Ignore());

        }
    }
}
