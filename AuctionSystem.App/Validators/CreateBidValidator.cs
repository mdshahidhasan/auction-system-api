using AuctionSystem.Core.Models.Bid;
using FluentValidation;

namespace AuctionSystem.App.Validators;

public class CreateBidValidator : AbstractValidator<BidWriteModel>
{
    public CreateBidValidator()
    {
        RuleFor(bid => bid.Amount)
            .GreaterThan(0).WithMessage("Bid amount must be greater than 0");
    }
}
