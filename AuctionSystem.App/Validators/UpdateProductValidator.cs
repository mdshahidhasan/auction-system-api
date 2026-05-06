using AuctionSystem.Core.Models.Product;
using FluentValidation;

namespace AuctionSystem.App.Validators;

public class UpdateProductValidator : AbstractValidator<ProductUpdateModel>
{
    private static readonly string[] AllowedPhotoTypes = ["image/jpeg", "image/png"];
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    public UpdateProductValidator()
    {
        RuleFor(product => product.Title)
            .NotEmpty().WithMessage("Title must not be empty")
            .MaximumLength(150).WithMessage("Title must be 150 characters or fewer")
            .When(product => product.Title is not null);

        RuleFor(product => product.Brand)
            .NotEmpty().WithMessage("Brand must not be empty")
            .MaximumLength(100).WithMessage("Brand must be 100 characters or fewer")
            .When(product => product.Brand is not null);

        RuleFor(product => product.Location)
            .NotEmpty().WithMessage("Location must not be empty")
            .MaximumLength(120).WithMessage("Location must be 120 characters or fewer")
            .When(product => product.Location is not null);

        RuleFor(product => product.Description)
            .NotEmpty().WithMessage("Description must not be empty")
            .When(product => product.Description is not null);

        RuleFor(product => product.AuctionEnds)
            .GreaterThan(product => product.AuctionStarts!.Value)
            .WithMessage("Auction end time must be later than auction start time")
            .When(product => product.AuctionEnds.HasValue && product.AuctionStarts.HasValue);

        RuleFor(product => product.AskingPrice)
            .GreaterThan(0).WithMessage("Asking price must be greater than 0")
            .When(product => product.AskingPrice.HasValue);

        RuleFor(product => product.StartingPrice)
            .GreaterThan(0).WithMessage("Starting price must be greater than 0")
            .When(product => product.StartingPrice.HasValue);

        RuleFor(product => product.MinBidIncrement)
            .GreaterThan(0).WithMessage("Minimum bid increment must be greater than 0")
            .When(product => product.MinBidIncrement.HasValue);

        RuleForEach(product => product.NewPhotos!)
            .Must(photo => photo.Length <= MaxPhotoBytes)
            .WithMessage("Photo must be less than or equal to 5MB")
            .Must(photo => AllowedPhotoTypes.Contains(photo.ContentType))
            .WithMessage("Photo must be in JPEG or PNG format")
            .When(product => product.NewPhotos is not null && product.NewPhotos.Any());
    }
}
