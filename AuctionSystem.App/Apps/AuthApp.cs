using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Interfaces.SecurityServices;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Auth;
using AuctionSystem.Core.Models.User;

namespace AuctionSystem.App.Apps;

public class AuthApp : IAuthApp
{
    private readonly IAuthService _authService;
    private readonly IUserApp _userApp;

    public AuthApp(IAuthService authService, IUserApp userApp)
    {
        _authService = authService;
        _userApp = userApp;
    }

    public Task<ServiceResult<UserReadModel>> Register(UserWriteModel model)
    {
        return _userApp.CreateUser(model);
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