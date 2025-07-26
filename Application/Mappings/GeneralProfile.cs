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
                .ForMember(dest => dest.SalePrice, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Stock))
                .ReverseMap()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.SalePrice))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Stock));

            CreateMap<ProductSaveViewModel, Product>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.SalePrice))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Stock))
                .ReverseMap()
                .ForMember(dest => dest.SalePrice, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Stock));

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
                .ForMember(dest => dest.CustomerId,
                           opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.PaymentMethod,
                           opt => opt.MapFrom(src => src.Payment.PaymentMethod))
                .ForMember(dest => dest.Payment,
                           opt => opt.MapFrom(src => src.Payment))
                .ForMember(dest => dest.Details,
                           opt => opt.MapFrom(src => src.Details));
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
                           opt => opt.MapFrom(src => "Completado"))
                // Explicitly map all payment fields
                .ForMember(dest => dest.CardNumber, opt => opt.MapFrom(src => src.CardNumber))
                .ForMember(dest => dest.CardHolderName, opt => opt.MapFrom(src => src.CardHolderName))
                .ForMember(dest => dest.CardExpiryDate, opt => opt.MapFrom(src => src.CardExpiryDate))
                .ForMember(dest => dest.CardCVV, opt => opt.MapFrom(src => src.CardCVV))
                .ForMember(dest => dest.TransferReference, opt => opt.MapFrom(src => src.TransferReference))
                .ForMember(dest => dest.BankAccount, opt => opt.MapFrom(src => src.BankAccount))
                .ForMember(dest => dest.TransferReceiptPath, opt => opt.MapFrom(src => src.TransferReceiptPath));
                           
            CreateMap<PaymentSaveViewModel, Payment>()
                .ForMember(dest => dest.Sale, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());


        }



    }
}
