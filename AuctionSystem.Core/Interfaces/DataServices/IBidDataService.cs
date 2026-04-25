using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Bid;
using System.Data;

namespace AuctionSystem.Core.Interfaces.Services;

public interface IBidDataService
{
    Task<Bid> CreateBid(Bid bid, IDbTransaction? transaction = null);
    Task<ServiceResult<List<Bid>>> GetBids(BidSearchModel searchModel);
    Task<Bid?> GetBidById(int id);
    Task<decimal?> GetHighestBidAmount(int productId, int? excludeBidId = null, IDbTransaction? transaction = null);
    Task<List<MyBidReadModel>> GetMyBids(int userId, IDbTransaction? transaction = null);
}