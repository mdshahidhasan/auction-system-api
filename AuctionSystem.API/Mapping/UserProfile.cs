using AutoMapper;
using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Models.User;
using AuctionSystem.Core.Models.Auth;

namespace AuctionSystem.API.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<AuthRegisterModel, User>();
        CreateMap<UserUpdateModel, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<User, UserReadModel>();
    }
}