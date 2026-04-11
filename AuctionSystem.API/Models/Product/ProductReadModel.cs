namespace AuctionSystem.API.Models.Product;

public class ProductReadModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string AuctionStatus { get; set; } = string.Empty;
    public DateTime AuctionStarts { get; set; }
    public DateTime AuctionEnds { get; set; }

    public decimal? AskingPrice { get; set; }
    public decimal? StartingPrice { get; set; }
    public decimal? CurrentHighestBid { get; set; }
    public decimal? MinBidIncrement { get; set; }
}
