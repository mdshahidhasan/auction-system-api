using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Interfaces.SecurityServices;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Auth;
using AuctionSystem.Core.Models.User;
using AuctionSystem.App.Validators;
using FluentValidation.Results;
using AuctionSystem.Core.Interfaces.Services;

namespace AuctionSystem.App.Apps;

public class AuthApp : IAuthApp
{
    private readonly IUserDataService _userDataService;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IUserApp _userApp;

    public AuthApp(IUserDataService userDataService, ITokenService tokenService, IPasswordService passwordService, IUserApp userApp)
    {
        _userDataService = userDataService;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _userApp = userApp;
    }

    public Task<ServiceResult<UserReadModel>> Register(UserWriteModel model)
    {
        return _userApp.CreateUser(model);
    }

    public async Task<ServiceResult<Token>> Login(AuthLoginModel model)
    {
        var validator = new LoginValidator();
        ValidationResult validationResult = validator.Validate(model);
        if (!validationResult.IsValid)
        {
            return new ServiceResult<Token>
            {
                Code = 400,
                Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            };
        }

        var user = await _userDataService.GetUserByEmail(model.Email);

        if (user is null)
        {
            return new ServiceResult<Token>
            {
                Code = 404,
                Message = "User not found."
            };
        }

        /*
        if (!user.IsVerified)
        {
            return new ServiceResult<Token>
            {
                Code = 403,
                Message = "Email not verified."
            };
        }*/

        bool passwordValid = _passwordService.VerifyPassword(model.Password, user.Password);

        if (!passwordValid)
        {
            return new ServiceResult<Token>
            {
                Code = 401,
                Message = "Invalid credentials."
            };
        }

        Token token = _tokenService.GenerateAuthToken(user, model.RememberMe);
        return new ServiceResult<Token>
        {
            Code = 200,
            Message = "Login successful.",
            Data = token
        };
    }

    /*
        public async Task<ServiceResult> VerifyEmail(AuthVerifyEmailModel model)
        {
            var validator = new VerifyEmailValidator();
            ValidationResult validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new ServiceResult
                {
                    Code = 400,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            var principal = _tokenService.ValidateToken(model.VerificationToken);
            var tokenUse = principal?.FindFirst("token_use")?.Value;
            var tokenEmail = principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;

            if (principal is null || tokenUse != "email_verification" ||
                !string.Equals(tokenEmail, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResult
                {
                    Code = 400,
                    Message = "Invalid verification code."
                };
            }

            return new ServiceResult
            {
                Code = 200,
                Message = "Email verified successfully."
            };
        }

        public async Task<ServiceResult> ForgotPassword(AuthForgotPasswordModel model)
        {
            var validator = new ForgotPasswordValidator();
            ValidationResult validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new ServiceResult
                {
                    Code = 400,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            var user = await _userDataService.GetUserByEmail(model.Email);
            if (user is null)
            {
                return new ServiceResult
                {
                    Code = 404,
                    Message = "User not found."
                };
            }

            string token = _tokenService.GeneratePasswordResetToken(user);

        }

        public async Task<ServiceResult> ResetPassword(AuthResetPasswordModel model)
        {
            var validator = new ResetPasswordValidator();
            ValidationResult validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                return new ServiceResult
                {
                    Code = 400,
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
                };
            }

            var principal = _tokenService.ValidateToken(model.Token);
            var tokenUse = principal?.FindFirst("token_use")?.Value;

            if (principal is null || tokenUse != "password_reset")
            {
                return new ServiceResult
                {
                    Code = 400,
                    Message = "Invalid or expired reset token."
                };
            }

            var claims = _tokenService.DecodeToken(model.Token);


        }
    */
    public async Task<ServiceResult> ChangePassword(int requesterUserId, AuthChangePasswordModel model)
    {
        var validator = new ChangePasswordValidator();
        ValidationResult validationResult = validator.Validate(model);
        if (!validationResult.IsValid)
        {
            return new ServiceResult
            {
                Code = 400,
                Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            };
        }

        var user = await _userDataService.GetUserById(requesterUserId);
        if (user is null)
        {
            return new ServiceResult
            {
                Code = 404,
                Message = "User not found."
            };
        }

        bool isCurrentPasswordValid = _passwordService.VerifyPassword(model.CurrentPassword, user.Password);

        if (!isCurrentPasswordValid)
        {
            return new ServiceResult
            {
                Code = 401,
                Message = "Current password is incorrect."
            };
        }

        if (!string.Equals(model.NewPassword, model.ConfirmPassword, StringComparison.Ordinal))
        {
            return new ServiceResult
            {
                Code = 400,
                Message = "NewPassword and ConfirmPassword do not match."
            };
        }


        user.Password = _passwordService.HashPassword(model.NewPassword);
        await _userDataService.UpdateUser(user);

        return new ServiceResult
        {
            Code = 200,
            Message = "Password changed successfully."
        };
    }

    public ServiceResult<Dictionary<string, string>> GetClaims(string token)
    {
        try
        {
            var claims = _tokenService.DecodeToken(token);

            if (claims is null)
            {
                return new ServiceResult<Dictionary<string, string>>
                {
                    Code = 400,
                    Message = "Failed to decode token or token is invalid."
                };
            }

            return new ServiceResult<Dictionary<string, string>>
            {
                Code = 200,
                Message = "Claims retrieved successfully.",
                Data = claims
            };
        }
        catch (Exception ex)
        {
            return new ServiceResult<Dictionary<string, string>>
            {
                Code = 500,
                Message = $"An error occurred while retrieving claims: {ex.Message}"
            };
        }
    }
}