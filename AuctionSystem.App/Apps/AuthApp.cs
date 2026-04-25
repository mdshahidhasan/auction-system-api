using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Interfaces.SecurityServices;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Auth;

namespace AuctionSystem.App.Apps;

public class AuthApp : IAuthApp
{
    private readonly IAuthService _authService;

    public AuthApp(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<ServiceResult<Token>> Login(AuthLoginModel model)
    {
        return _authService.Login(model);
    }

    public Task<ServiceResult> VerifyEmail(AuthVerifyEmailModel model)
    {
        return _authService.VerifyEmail(model);
    }

    public Task<ServiceResult> ForgotPassword(AuthForgotPasswordModel model)
    {
        return _authService.ForgotPassword(model);
    }

    public Task<ServiceResult> ResetPassword(AuthResetPasswordModel model)
    {
        return _authService.ResetPassword(model);
    }

    public Task<ServiceResult> ChangePassword(int requesterUserId, AuthChangePasswordModel model)
    {
        return _authService.ChangePassword(model);
    }
}