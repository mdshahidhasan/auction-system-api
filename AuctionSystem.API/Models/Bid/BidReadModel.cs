namespace AuctionSystem.API.Models.Bid;

public class BidReadModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }
    public string? UserFullName { get; set; }
    public int ProductId { get; set; }
    public decimal Amount { get; set; }
}
