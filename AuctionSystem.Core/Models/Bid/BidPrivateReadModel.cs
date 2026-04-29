using AuctionSystem.Core.Models.User;

namespace AuctionSystem.Core.Models.Bid;

public class BidPrivateReadModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ProductId { get; set; }
    public decimal Amount { get; set; }

    public UserReadModel User { get; set; } = null!;
}
