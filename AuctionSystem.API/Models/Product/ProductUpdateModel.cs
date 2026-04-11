namespace AuctionSystem.API.Models.Product;

public class ProductUpdateModel
{
    public string? Title { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public DateTime? AuctionStarts { get; set; }
    public DateTime? AuctionEnds { get; set; }
    public decimal? AskingPrice { get; set; }
    public decimal? StartingPrice { get; set; }
    public decimal? MinBidIncrement { get; set; }
}
