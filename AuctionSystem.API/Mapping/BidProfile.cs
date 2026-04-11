using AutoMapper;
using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Models.Bid;

namespace AuctionSystem.API.Mapping;

public class BidProfile : Profile
{
    public BidProfile()
    {
        CreateMap<BidWriteModel, Bid>();
        CreateMap<Bid, BidReadModel>();
        CreateMap<Bid, BidPublicReadModel>();
    }
}