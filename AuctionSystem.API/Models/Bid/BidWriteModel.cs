namespace AuctionSystem.API.Models.Bid;

public class BidWriteModel
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public decimal Amount { get; set; }
}
