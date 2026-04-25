using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.User;
using System.Data;

namespace AuctionSystem.Core.Interfaces.Services;

public interface IUserDataService
{
    Task<User> CreateUser(User user, IDbTransaction? transaction = null);
    Task<ServiceResult<List<User>>> GetUsers(UserSearchModel searchModel);
    Task<User?> GetUserByEmail(string email, IDbTransaction? transaction = null);
    Task<User?> GetUserById(int userId);
    Task UpdateUser(User user);
    Task DeleteUser(int userId);
}