namespace AuctionSystem.Core.Models.Bid;

public class MyBidReadModel
{
    public int BidId { get; set; }
    public decimal MyBidAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public string ProductBrand { get; set; } = string.Empty;
    public string? PrimaryPhotoUrl { get; set; }
    public decimal? CurrentHighestBid { get; set; }
    public decimal MinBidIncrement { get; set; }
    public DateTime AuctionEnds { get; set; }
    public string AuctionStatus { get; set; } = string.Empty;
    public bool IsWinning { get; set; }
}