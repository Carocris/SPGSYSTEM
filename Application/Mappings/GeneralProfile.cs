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

            // Product
            CreateMap<Product, ProductViewModel>()
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Stock))
                .ReverseMap()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.StockQuantity));

            CreateMap<ProductSaveViewModel, Product>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.StockQuantity))
                .ReverseMap()
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.StockQuantity, opt => opt.MapFrom(src => src.Stock));

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
                           opt => opt.MapFrom(src => src.Payment.PaymentMethod));
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
