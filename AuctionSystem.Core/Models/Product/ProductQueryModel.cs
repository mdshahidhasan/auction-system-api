using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.Core.Models.Product;

public class ProductQueryModel
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public string? AuctionStatus { get; set; }
}
