using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.User;

namespace AuctionSystem.Core.Interfaces.Apps;

public interface IUserApp
{
    Task<ServiceResult<UserReadModel>> CreateUser(UserWriteModel model);
    Task<ServiceResult<UserReadModel>> GetUserById(int requesterUserId);
    Task<ServiceResult> UpdateUser(int requesterUserId, UserUpdateModel userUpdateModel);
    Task<ServiceResult> DeleteUser(int requesterUserId);
}