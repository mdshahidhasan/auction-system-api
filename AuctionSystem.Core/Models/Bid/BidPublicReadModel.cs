namespace AuctionSystem.Core.Models.Bid;

public class BidPublicReadModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int UserId { get; set; }
    public string? UserFullName { get; set; }
    public int ProductId { get; set; }
    public decimal Amount { get; set; }
}