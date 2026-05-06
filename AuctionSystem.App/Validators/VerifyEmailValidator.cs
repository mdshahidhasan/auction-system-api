using AuctionSystem.Core.Models.Auth;
using FluentValidation;

namespace AuctionSystem.App.Validators;

public class VerifyEmailValidator : AbstractValidator<AuthVerifyEmailModel>
{
    public VerifyEmailValidator()
    {
        RuleFor(m => m.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(m => m.VerificationToken)
            .NotEmpty().WithMessage("Verification token is required");
    }
}
