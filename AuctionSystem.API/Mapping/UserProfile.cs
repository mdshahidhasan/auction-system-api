using AutoMapper;
using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Models.User;

namespace AuctionSystem.API.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserUpdateModel, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<User, UserReadModel>();
    }
}