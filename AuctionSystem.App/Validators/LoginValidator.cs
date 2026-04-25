using AuctionSystem.Core.Models.Auth;
using FluentValidation;

namespace AuctionSystem.App.Validators;

public class LoginValidator : AbstractValidator<AuthLoginModel>
{
    public LoginValidator()
    {
        RuleFor(model => model.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(model => model.Password)
            .NotEmpty().WithMessage("Password is required");

        RuleFor(model => model.Role)
            .Must(role => string.IsNullOrWhiteSpace(role) || role is "User" or "Admin")
            .WithMessage("Role must be User or Admin");
    }
}
