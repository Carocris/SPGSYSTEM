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

namespace Application.Mappings
{
    public class GeneralProfile : Profile
    {

        public GeneralProfile() {

            // Customer
            CreateMap<Customer, CustomerViewModel>()
                .ReverseMap();  

            CreateMap<CustomerSaveViewModel, Customer>()
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
                .ReverseMap()
                .ForMember(dest => dest.Price, opt => opt.Ignore()); // Evitar conflicto con SalePrice

            // SaleDetail
            CreateMap<SaleDetail, SaleDetailViewModel>()
                .ForMember(dest => dest.ProductName,
                           opt => opt.MapFrom(src => src.Product.Name));

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
                .ReverseMap();
            CreateMap<PaymentSaveViewModel, Payment>()
                .ForMember(dest => dest.PaymentDate, opt => opt.Ignore())
                .ForMember(dest => dest.Sale, opt => opt.Ignore());

        }
    }
}
