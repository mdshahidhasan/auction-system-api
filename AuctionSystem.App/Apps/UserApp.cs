using AutoMapper;
using AuctionSystem.App.Validators;
using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Interfaces.SecurityServices;
using AuctionSystem.Core.Interfaces.Services;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.User;

namespace AuctionSystem.App.Apps;

public class UserApp : IUserApp
{
    private readonly IUserDataService _userService;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserApp(IUserDataService userService, IPasswordService passwordService, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _userService = userService;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<UserReadModel>> CreateUser(UserWriteModel model)
    {
        var createUserValidator = new CreateUserValidator();
        var validationResult = createUserValidator.Validate(model);
        if (!validationResult.IsValid)
        {
            return new ServiceResult<UserReadModel>
            {
                Code = 400,
                Message = string.Join("; ", validationResult.Errors.Select(error => error.ErrorMessage))
            };
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var existingUser = await _userService.GetUserByEmail(model.Email, _unitOfWork.Transaction);
            if (existingUser is not null)
            {
                await _unitOfWork.RollbackAsync();

                return new ServiceResult<UserReadModel>
                {
                    Code = 409,
                    Message = "Already have an account"
                };
            }

            var user = _mapper.Map<User>(model);
            user.Password = _passwordService.HashPassword(model.Password);
            user.Role = "Customer";
            user.IsActive = true;
            user.IsVerified = false;

            var createdUser = await _userService.CreateUser(user, _unitOfWork.Transaction);
            await _unitOfWork.CommitAsync();

            var userReadModel = _mapper.Map<UserReadModel>(createdUser);

            return new ServiceResult<UserReadModel>
            {
                Code = 201,
                Message = "User created successfully.",
                Data = userReadModel
            };
        }
        catch
        {
            await _unitOfWork.RollbackAsync();

            return new ServiceResult<UserReadModel>
            {
                Code = 400,
                Message = "Signup Failed"
            };
        }
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

    public async Task<ServiceResult<UserReadModel>> GetUserById(int requesterUserId)
    {
        User? user = await _userService.GetUserById(requesterUserId);
        if (user is null)
        {
            return new ServiceResult<UserReadModel>
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

    public async Task<ServiceResult> UpdateUser(int requesterUserId, UserUpdateModel userUpdateModel)
    {
        User? user = await _userService.GetUserById(requesterUserId);
        if (user == null)
        {
            return new ServiceResult
            {
                Code = 404,
                Message = "User not found."
            };
        }

        user = _mapper.Map(userUpdateModel, user);

        user.UpdatedAt = DateTime.UtcNow;

        //Needs to implement photo update

        await _userService.UpdateUser(user);

        User? updatedUser = await _userService.GetUserById(requesterUserId);
        var userReadModel = _mapper.Map<UserReadModel>(updatedUser);

        return new ServiceResult<UserReadModel>
        {
            Data = userReadModel,
            Code = 200,
            Message = "User updated successfully."
        };
    }

    public async Task<ServiceResult> DeleteUser(int requesterUserId)
    {
        User? user = await _userService.GetUserById(requesterUserId);
        if (user == null)
        {
            return new ServiceResult
            {
                Code = 404,
                Message = "User not found."
            };
        }

        await _userService.DeleteUser(requesterUserId);

        return new ServiceResult
        {
            Code = 200,
            Message = "User deleted successfully."
        };
    }
}
