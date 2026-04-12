using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Auth;
using AuctionSystem.Core.Models.User;

namespace AuctionSystem.Core.Interfaces.Apps;

public interface IAuthApp
{
    Task<ServiceResult<UserReadModel>> Register(AuthRegisterModel model);
    Task<ServiceResult<Token>> Login(AuthLoginModel model);
    Task<ServiceResult> VerifyEmail(AuthVerifyEmailModel model);
    Task<ServiceResult> ForgotPassword(AuthForgotPasswordModel model);
    Task<ServiceResult> ResetPassword(AuthResetPasswordModel model);
    Task<ServiceResult> ChangePassword(int requesterUserId, AuthChangePasswordModel model);
}