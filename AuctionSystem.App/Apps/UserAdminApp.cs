using AutoMapper;
using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Interfaces.Services;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.User;

namespace AuctionSystem.App.Apps;

public class UserAdminApp : IUserAdminApp
{
    private readonly IUserDataService _userService;
    private readonly IMapper _mapper;

    public UserAdminApp(IUserDataService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    public async Task<ServiceResult<List<UserReadModel>>> GetUsers(UserQueryModel queryModel)
    {
        UserSearchModel searchModel = new UserSearchModel
        {
            Email = queryModel.Email,
            Role = queryModel.Role,
            Limit = queryModel.Limit,
            Offset = (queryModel.Page - 1) * queryModel.Limit
        };

        ServiceResult<List<User>> users = await _userService.GetUsers(searchModel);
        var userReadModels = _mapper.Map<List<UserReadModel>>(users.Data ?? new List<User>());

        return new ServiceResult<List<UserReadModel>>
        {
            Code = users.Code,
            Message = users.Message,
            Data = userReadModels,
            TotalCount = users.TotalCount
        };
    }

    public async Task<ServiceResult<UserReadModel>> GetUserById(int userId)
    {
        User? user = await _userService.GetUserById(userId);
        if (user is null)
        {
            return new ServiceResult<UserReadModel>
            {
                Code = 404,
                Message = "User not found."
            };
        }

        var userReadModel = _mapper.Map<UserReadModel>(user);

        return new ServiceResult<UserReadModel>
        {
            Code = 200,
            Message = "User retrieved successfully.",
            Data = userReadModel
        };
    }

    public async Task<ServiceResult> UpdateUserById(int userId, UserAdminUpdateModel userUpdateModel)
    {
        User? user = await _userService.GetUserById(userId);
        if (user == null)
        {
            return new ServiceResult
            {
                Code = 404,
                Message = "User not found."
            };
        }

        user = _mapper.Map(userUpdateModel, user);

        await _userService.UpdateUser(user);

        User? updatedUser = await _userService.GetUserById(userId);
        var userReadModel = _mapper.Map<UserReadModel>(updatedUser);

        return new ServiceResult<UserReadModel>
        {
            Data = userReadModel,
            Code = 200,
            Message = "User updated successfully."
        };
    }

    public async Task<ServiceResult> DeleteUserById(int userId)
    {
        User? user = await _userService.GetUserById(userId);
        if (user == null)
        {
            return new ServiceResult
            {
                Code = 404,
                Message = "User not found."
            };
        }

        var userReadModels = _mapper.Map<UserReadModel>(user);

        return new ServiceResult<UserReadModel>
        {
            Code = 200,
            Message = "User retrieved successfully.",
            Data = userReadModels
        };
    }
}
