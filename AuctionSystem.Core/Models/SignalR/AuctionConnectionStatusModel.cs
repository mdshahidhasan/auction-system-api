namespace AuctionSystem.Core.Models.SignalR;

public class AuctionConnectionStatusModel
{
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public string UserDisplayName { get; set; } = string.Empty;
    public DateTime StatusChangedUtcTime { get; set; }
    public string ConnectionStatus { get; set; } = string.Empty;
    public int ActiveWatchersCount { get; set; }
}
