namespace AuctionSystem.Core.Entities;

public class Bid : BaseEntity
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public decimal Amount { get; set; }
}