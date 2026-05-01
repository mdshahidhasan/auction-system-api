using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Interfaces.Services;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Bid;
using AutoMapper;
using AuctionSystem.Core.Entities;
using AuctionSystem.App.Validators;
using AuctionSystem.Core.Models.User;

namespace AuctionSystem.App.Apps;

public class BidApp : IBidApp
{
    private readonly IBidDataService _bidService;
    private readonly IProductDataService _productService;
    private readonly IUserDataService _userService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BidApp(IBidDataService bidService, IProductDataService productService, IUserDataService userService, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _bidService = bidService;
        _productService = productService;
        _userService = userService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<BidReadModel>> CreateBid(int userId, int productId, BidWriteModel bidWriteModel)
    {
        var createBidValidator = new CreateBidValidator();
        var validationResult = createBidValidator.Validate(bidWriteModel);
        if (!validationResult.IsValid)
        {
            return new ServiceResult<BidReadModel>
            {
                Code = 400,
                Message = string.Join("; ", validationResult.Errors.Select(error => error.ErrorMessage))
            };
        }

        var product = await _productService.GetProductById(productId);
        if (product is null)
        {
            return new ServiceResult<BidReadModel>
            {
                Code = 404,
                Message = "Product not found."
            };
        }

        if (product.UserId == userId)
        {
            return new ServiceResult<BidReadModel>
            {
                Code = 403,
                Message = "Forbidden: You cannot place a bid on your own product."
            };
        }

        if (!IsAuctionActive(product))
        {
            return new ServiceResult<BidReadModel>
            {
                Code = 400,
                Message = "Bidding is available only while the auction is active."
            };
        }

        var highestBid = await _bidService.GetHighestBidAmount(productId);
        var minimumAllowedAmount = highestBid.HasValue
            ? highestBid.Value + product.MinBidIncrement
            : (product.StartingPrice ?? 0);

        if (bidWriteModel.Amount < minimumAllowedAmount)
        {
            return new ServiceResult<BidReadModel>
            {
                Code = 400,
                Message = $"Bid amount must be at least {minimumAllowedAmount}."
            };
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var bid = _mapper.Map<Bid>(bidWriteModel);
            bid.UserId = userId;
            bid.ProductId = productId;

            var createdBid = await _bidService.CreateBid(bid, _unitOfWork.Transaction);
            await _productService.UpdateCurrentHighestBid(productId, createdBid.Amount, _unitOfWork.Transaction);

            await _unitOfWork.CommitAsync();

            var bidReadModel = _mapper.Map<BidReadModel>(createdBid);

            return new ServiceResult<BidReadModel>
            {
                Code = 201,
                Message = "Bid created successfully.",
                Data = bidReadModel
            };
        }
        catch
        {
            await _unitOfWork.RollbackAsync();

            return new ServiceResult<BidReadModel>
            {
                Code = 400,
                Message = "Bid creation failed."
            };
        }
    }

    public async Task<ServiceResult<List<BidReadModel>>> GetBids(int productId, BidQueryModel queryModel)
    {
        var searchModel = new BidSearchModel
        {
            ProductId = productId,
            Limit = queryModel.Limit,
            Offset = (queryModel.Page - 1) * queryModel.Limit
        };

        ServiceResult<List<Bid>> bids = await _bidService.GetBids(searchModel);
        var BidReadModels = _mapper.Map<List<BidReadModel>>(bids.Data ?? new List<Bid>());

        foreach (var bidReadModel in BidReadModels)
        {
            var bidder = await _userService.GetUserById(bidReadModel.UserId);
            bidReadModel.UserFullName = bidder != null ? $"{bidder.FirstName} {bidder.LastName}" : "Unknown User";
            bidReadModel.BidderPhotoUrl = bidder?.PhotoPath ?? string.Empty;
        }

        return new ServiceResult<List<BidReadModel>>
        {
            Code = bids.Code,
            Message = bids.Message,
            Data = BidReadModels,
            TotalCount = bids.TotalCount
        };
    }

    public async Task<ServiceResult<List<BidPrivateReadModel>>> GetPrivateBids(int requesterUserId, string? requesterRole, int productId, BidQueryModel queryModel)
    {
        var product = await _productService.GetProductById(productId);

        if (product is null)
        {
            return new ServiceResult<List<BidPrivateReadModel>>
            {
                Code = 404,
                Message = "Product not found."
            };
        }

        if (product.UserId != requesterUserId && !IsAdmin(requesterRole))
        {
            return new ServiceResult<List<BidPrivateReadModel>>
            {
                Code = 403,
                Message = "Forbidden: You do not have permission to view detailed bids for this product."
            };
        }

        var searchModel = new BidSearchModel
        {
            ProductId = productId,
            Limit = queryModel.Limit,
            Offset = (queryModel.Page - 1) * queryModel.Limit
        };

        ServiceResult<List<Bid>> bids = await _bidService.GetBids(searchModel);

        var bidReadModels = _mapper.Map<List<BidPrivateReadModel>>(bids.Data ?? new List<Bid>());

        foreach (var bidReadModel in bidReadModels)
        {
            var bidder = await _userService.GetUserById(bidReadModel.Id);
            var bidderReadModel = _mapper.Map<UserReadModel>(bidder);
            bidReadModel.User = bidderReadModel;
        }

        return new ServiceResult<List<BidPrivateReadModel>>
        {
            Code = bids.Code,
            Message = bids.Message,
            Data = bidReadModels,
            TotalCount = bids.TotalCount
        };
    }

    public async Task<ServiceResult<List<MyBidReadModel>>> GetMyBids(int userId)
    {
        var bids = await _bidService.GetMyBids(userId);

        return new ServiceResult<List<MyBidReadModel>>
        {
            Code = 200,
            Message = "My bids retrieved successfully.",
            Data = bids
        };
    }


    private static bool IsAuctionActive(Product product)
    {
        var now = DateTime.UtcNow;

        return string.Equals(product.AuctionStatus, "Active", StringComparison.OrdinalIgnoreCase)
            && product.AuctionStarts <= now
            && product.AuctionEnds > now;
    }

    private static bool IsAdmin(string? requesterRole)
    {
        return string.Equals(requesterRole, "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
