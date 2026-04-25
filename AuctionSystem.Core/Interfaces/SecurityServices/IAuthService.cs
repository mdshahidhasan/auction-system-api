using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Auth;
using AuctionSystem.Core.Models.User;

namespace AuctionSystem.Core.Interfaces.SecurityServices;

public interface IAuthService
{
    Task<ServiceResult<Token>> Login(AuthLoginModel model);
    Task<ServiceResult> VerifyEmail(AuthVerifyEmailModel model);
    Task<ServiceResult> ForgotPassword(AuthForgotPasswordModel model);
    Task<ServiceResult> ResetPassword(AuthResetPasswordModel model);
    Task<ServiceResult> ChangePassword(AuthChangePasswordModel model);
}