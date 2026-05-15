namespace AuctionSystem.Core.Models.SignalR;

public class AuctionBidUpdateModel
{
    public int BidId { get; set; }
    public int ProductId { get; set; }
    public int BidderId { get; set; }
    public string BidderDisplayName { get; set; } = string.Empty;
    public decimal BidAmount { get; set; }
    public DateTime BidPlacedUtcTime { get; set; }
    public int BidSequenceNumber { get; set; }
    public bool IsCurrentHighestBid { get; set; }
}
