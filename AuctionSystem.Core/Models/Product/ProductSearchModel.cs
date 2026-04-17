using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.Core.Models.Product;

public class ProductSearchModel
{
    public int Limit { get; set; } = 10;
    public int Offset { get; set; }
    public string? AuctionStatus { get; set; }
}
