using Microsoft.AspNetCore.Http;

namespace AuctionSystem.Core.Models.Product;

public class ProductWriteModel
{
    public string Title { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime? AuctionStarts { get; set; }
    public DateTime AuctionEnds { get; set; }

    public decimal? AskingPrice { get; set; }
    public decimal? StartingPrice { get; set; }
    public decimal? MinBidIncrement { get; set; }

    public List<IFormFile> Photos { get; set; } = new();
}
