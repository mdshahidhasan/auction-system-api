namespace AuctionSystem.Core.Entities;

public class ProductPhoto : BaseEntity
{
    public int ProductId { get; set; }
    public int PhotoOrder { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
}