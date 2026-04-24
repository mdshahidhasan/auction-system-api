using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.Core.Models.Bid;

public class BidSearchModel
{
    public int ProductId { get; set; }
    public int Limit { get; set; } = 10;
    public int Offset { get; set; }
}
