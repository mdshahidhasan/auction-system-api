using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.Core.Models.Bid;

public class BidSearchModel
{
    [Range(1, 100, ErrorMessage = "Limit must be between 1 and 100.")]
    public int Limit { get; set; } = 10;

    [Range(0, int.MaxValue, ErrorMessage = "Offset must be 0 or greater.")]
    public int Offset { get; set; }
}
