using AuctionSystem.Core.Models.Auth;
using FluentValidation;

namespace AuctionSystem.App.Validators;

public class ChangePasswordValidator : AbstractValidator<AuthChangePasswordModel>
{
    public ChangePasswordValidator()
    {
        RuleFor(m => m.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(m => m.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[\d\W\s]").WithMessage("Password must contain at least one number, symbol, or whitespace");

        RuleFor(m => m.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(m => m.NewPassword).WithMessage("Password and ConfirmPassword do not match");
    }
}
