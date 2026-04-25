using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Bid;

namespace AuctionSystem.Core.Interfaces.Apps;

public interface IBidApp
{
    Task<ServiceResult<BidReadModel>> CreateBid(int userId, int productId, BidWriteModel bidWriteModel);
    Task<ServiceResult<List<BidPublicReadModel>>> GetPublicBids(int productId, BidQueryModel queryModel);
    Task<ServiceResult<List<BidReadModel>>> GetBids(int productId, int requesterUserId, string? requesterRole, BidQueryModel queryModel);
    Task<ServiceResult<List<MyBidReadModel>>> GetMyBids(int userId);
}