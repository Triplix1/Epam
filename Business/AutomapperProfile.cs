using AutoMapper;
using Business.Models;
using Data.Entities;
using System;
using System.Linq;

namespace Business
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<Receipt, ReceiptModel>()
                .ForMember(rm => rm.ReceiptDetailsIds, r => r.MapFrom(x => x.ReceiptDetails.Select(rd => rd.Id)))
                .ReverseMap();

            CreateMap<Product, ProductModel>()
                .ForMember(rm => rm.ReceiptDetailIds, r => r.MapFrom(x => x.ReceiptDetails.Select(rd => rd.Id)))
                .ForMember(rm => rm.CategoryName, r => r.MapFrom(x => x.Category.CategoryName))
                .ReverseMap();

            CreateMap<ReceiptDetail, ReceiptDetailModel>()
                .ReverseMap();

            CreateMap<Tuple<Person, Customer>, CustomerModel>()
                .ForMember(p => p.ReceiptsIds, r => r.MapFrom(x => x.Item2.Receipts.Select(rd => rd.Id)))
                .ReverseMap();

            CreateMap<Customer, CustomerModel>()
                .ForMember(p => p.ReceiptsIds, r => r.MapFrom(x => x.Receipts.Select(rd => rd.Id)))
                .ForMember(p => p.Name, c => c.MapFrom(x => x.Person.Name))
                .ForMember(p => p.BirthDate, c => c.MapFrom(x => x.Person.BirthDate))
                .ForMember(p => p.Surname, c => c.MapFrom(x => x.Person.Surname))
                .ReverseMap();

            CreateMap<Person, CustomerModel>()
                .ReverseMap();

            CreateMap<ProductCategory, ProductCategoryModel>()
                .ForMember(p => p.ProductIds, r => r.MapFrom(x => x.Products.Select(rd => rd.Id)))
                .ReverseMap();

        }
    }
}