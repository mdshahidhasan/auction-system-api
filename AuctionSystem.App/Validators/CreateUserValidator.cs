using AuctionSystem.Core.Models.User;
using FluentValidation;

namespace AuctionSystem.App.Validators;

public class CreateUserValidator : AbstractValidator<UserWriteModel>
{
    public CreateUserValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(user => user.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[\d\W\s]").WithMessage("Password must contain at least one number, symbol, or whitespace");

        RuleFor(user => user.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(user => user.Password).WithMessage("Password and ConfirmPassword do not match");

        RuleFor(user => user.FirstName)
            .NotEmpty().WithMessage("First name is required");

        RuleFor(user => user.LastName)
            .NotEmpty().WithMessage("Last name is required");

        RuleFor(user => user.ContactNumber)
            .Matches(@"^\+?\d{10,15}$").WithMessage("Contact number must be a valid phone number")
            .When(user => !string.IsNullOrWhiteSpace(user.ContactNumber));
    }
}
