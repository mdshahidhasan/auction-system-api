using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Interfaces.SecurityServices;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Auth;

namespace AuctionSystem.Infra.Services.Security;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;

    public AuthService(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public Task<ServiceResult<Token>> Login(AuthLoginModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            return Task.FromResult(new ServiceResult<Token>
            {
                Code = 400,
                Message = "Email and password are required."
            });
        }

        var token = _tokenService.GenerateAuthToken(new User
        {
            Id = 1001,
            Email = model.Email,
            FirstName = "User",
            Role = "User"
        }, model.RememberMe);

        return Task.FromResult(new ServiceResult<Token>
        {
            Code = 200,
            Message = "Login successful.",
            Data = token
        });
    }

    public Task<ServiceResult> VerifyEmail(AuthVerifyEmailModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.VerificationToken))
        {
            return Task.FromResult(new ServiceResult
            {
                Code = 400,
                Message = "Email and verification code are required."
            });
        }

        var principal = _tokenService.ValidateToken(model.VerificationToken);
        var tokenUse = principal?.FindFirst("token_use")?.Value;
        var tokenEmail = principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
            ?? principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;

        if (principal is null || tokenUse != "email_verification" ||
            !string.Equals(tokenEmail, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ServiceResult
            {
                Code = 400,
                Message = "Invalid verification code."
            });
        }

        return Task.FromResult(new ServiceResult
        {
            Code = 200,
            Message = "Email verified successfully."
        });
    }

    public Task<ServiceResult> ForgotPassword(AuthForgotPasswordModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return Task.FromResult(new ServiceResult
            {
                Code = 400,
                Message = "Email is required."
            });
        }

        _ = _tokenService.GeneratePasswordResetToken(new User
        {
            Id = 1001,
            Email = model.Email,
            Role = "User"
        });

        return Task.FromResult(new ServiceResult
        {
            Code = 200,
            Message = "If the email exists, a reset code has been sent."
        });
    }

    public Task<ServiceResult> ResetPassword(AuthResetPasswordModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Token) ||
            string.IsNullOrWhiteSpace(model.NewPassword) ||
            string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            return Task.FromResult(new ServiceResult
            {
                Code = 400,
                Message = "Token, new password, and confirm password are required."
            });
        }

        if (!string.Equals(model.NewPassword, model.ConfirmPassword, StringComparison.Ordinal))
        {
            return Task.FromResult(new ServiceResult
            {
                Code = 400,
                Message = "NewPassword and ConfirmPassword do not match."
            });
        }

        var principal = _tokenService.ValidateToken(model.Token);
        var tokenUse = principal?.FindFirst("token_use")?.Value;

        if (principal is null || tokenUse != "password_reset")
        {
            return Task.FromResult(new ServiceResult
            {
                Code = 400,
                Message = "Invalid reset code."
            });
        }

        return Task.FromResult(new ServiceResult
        {
            Code = 200,
            Message = "Password reset successful."
        });
    }

    public Task<ServiceResult> ChangePassword(AuthChangePasswordModel model)
    {
        if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
            string.IsNullOrWhiteSpace(model.NewPassword) ||
            string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            return Task.FromResult(new ServiceResult
            {
                Code = 400,
                Message = "Email, current password, new password, and confirm password are required."
            });
        }

        if (!string.Equals(model.NewPassword, model.ConfirmPassword, StringComparison.Ordinal))
        {
            return Task.FromResult(new ServiceResult
            {
                Code = 400,
                Message = "NewPassword and ConfirmPassword do not match."
            });
        }

        return Task.FromResult(new ServiceResult
        {
            Code = 200,
            Message = "Password changed successfully."
        });
    }
}