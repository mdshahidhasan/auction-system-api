using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.User;

namespace AuctionSystem.Core.Interfaces.Apps;

public interface IUserApp
{
    Task<ServiceResult<List<UserReadModel>>> GetUsers(UserQueryModel queryModel);
    Task<ServiceResult<UserReadModel>> GetUserById(int requesterUserId, int userId, string? requesterRole);
    Task<ServiceResult> UpdateUser(int requesterUserId, int userId, UserUpdateModel userUpdateModel);
    Task<ServiceResult> DeleteUser(int requesterUserId, int userId);
}