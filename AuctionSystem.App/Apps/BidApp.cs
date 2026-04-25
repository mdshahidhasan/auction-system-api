using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Interfaces.Services;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Bid;
using AuctionSystem.Core.Models.User;
using AutoMapper;
using AuctionSystem.Core.Entities;
using AuctionSystem.App.Validators;

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

            var bidReadModel = await BuildBidReadModel(createdBid);

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

    public async Task<ServiceResult<List<BidPublicReadModel>>> GetPublicBids(int productId, BidQueryModel queryModel)
    {
        var searchModel = new BidSearchModel
        {
            ProductId = productId,
            Limit = queryModel.Limit,
            Offset = (queryModel.Page - 1) * queryModel.Limit
        };

        ServiceResult<List<Bid>> bids = await _bidService.GetBids(searchModel);
        var publicBidReadModels = await BuildBidPublicReadModels(bids.Data ?? new List<Bid>());

        return new ServiceResult<List<BidPublicReadModel>>
        {
            Code = bids.Code,
            Message = bids.Message,
            Data = publicBidReadModels,
            TotalCount = bids.TotalCount
        };
    }

    public async Task<ServiceResult<List<BidReadModel>>> GetBids(int productId, int requesterUserId, string? requesterRole, BidQueryModel queryModel)
    {
        var product = await _productService.GetProductById(productId);
        if (product is null)
        {
            return new ServiceResult<List<BidReadModel>>
            {
                Code = 404,
                Message = "Product not found."
            };
        }

        if (product.UserId != requesterUserId && !IsAdmin(requesterRole))
        {
            return new ServiceResult<List<BidReadModel>>
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
        var bidReadModels = await BuildBidReadModels(bids.Data ?? new List<Bid>());

        return new ServiceResult<List<BidReadModel>>
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

        foreach (var bid in bids)
        {
            bid.IsWinning = bid.CurrentHighestBid.HasValue && bid.MyBidAmount == bid.CurrentHighestBid.Value;
        }

        return new ServiceResult<List<MyBidReadModel>>
        {
            Code = 200,
            Message = "My bids retrieved successfully.",
            Data = bids
        };
    }

    private async Task<BidReadModel> BuildBidReadModel(Bid bid)
    {
        var bidReadModel = _mapper.Map<BidReadModel>(bid);
        var user = await _userService.GetUserById(bid.UserId);

        bidReadModel.User = user is null
            ? new UserReadModel()
            : _mapper.Map<UserReadModel>(user);

        return bidReadModel;
    }

    private async Task<List<BidReadModel>> BuildBidReadModels(List<Bid> bids)
    {
        var users = new Dictionary<int, UserReadModel?>();
        var bidReadModels = new List<BidReadModel>();

        foreach (var bid in bids)
        {
            if (!users.ContainsKey(bid.UserId))
            {
                var user = await _userService.GetUserById(bid.UserId);
                users[bid.UserId] = user is null ? null : _mapper.Map<UserReadModel>(user);
            }

            var bidReadModel = _mapper.Map<BidReadModel>(bid);
            bidReadModel.User = users[bid.UserId] ?? new UserReadModel();

            bidReadModels.Add(bidReadModel);
        }

        return bidReadModels;
    }

    private async Task<List<BidPublicReadModel>> BuildBidPublicReadModels(List<Bid> bids)
    {
        var publicBidReadModels = _mapper.Map<List<BidPublicReadModel>>(bids);
        var users = new Dictionary<int, string?>();

        for (var i = 0; i < bids.Count; i++)
        {
            var bid = bids[i];

            if (!users.ContainsKey(bid.UserId))
            {
                var user = await _userService.GetUserById(bid.UserId);
                users[bid.UserId] = user is null ? null : $"{user.FirstName} {user.LastName}".Trim();
            }

            publicBidReadModels[i].UserFullName = users[bid.UserId];
        }

        return publicBidReadModels;
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
