using AuctionSystem.Core.Models.SignalR;

namespace AuctionSystem.Core.Interfaces.ExternalServices;

public interface IAuctionRealtimeNotificationService
{
    Task BroadcastNewBidAsync(AuctionBidUpdateModel bidUpdate);
    Task JoinAuctionGroupAsync(string connectionId, int productId);
    Task LeaveAuctionGroupAsync(string connectionId, int productId);
}
