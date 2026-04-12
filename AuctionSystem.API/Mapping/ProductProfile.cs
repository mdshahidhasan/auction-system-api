using AutoMapper;
using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Models.Product;

namespace AuctionSystem.API.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<ProductWriteModel, Product>();
        CreateMap<ProductUpdateModel, Product>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Product, ProductReadModel>();
    }
}