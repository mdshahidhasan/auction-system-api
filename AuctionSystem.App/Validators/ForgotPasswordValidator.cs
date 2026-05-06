using AuctionSystem.Core.Models.Auth;
using FluentValidation;

namespace AuctionSystem.App.Validators;

public class ForgotPasswordValidator : AbstractValidator<AuthForgotPasswordModel>
{
    public ForgotPasswordValidator()
    {
        RuleFor(m => m.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");
    }
}
