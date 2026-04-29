using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.User;

namespace AuctionSystem.Core.Interfaces.Apps;

public interface IUserAdminApp
{
    Task<ServiceResult<List<UserReadModel>>> GetUsers(UserQueryModel queryModel);
    Task<ServiceResult<UserReadModel>> GetUserById(int userId);
    Task<ServiceResult> UpdateUserById(int userId, UserAdminUpdateModel userUpdateModel);
    Task<ServiceResult> DeleteUserById(int userId);
}