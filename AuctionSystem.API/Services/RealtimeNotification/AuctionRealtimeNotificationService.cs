using Microsoft.AspNetCore.SignalR;
using AuctionSystem.API.Helpers;
using AuctionSystem.API.Hubs;
using AuctionSystem.Core.Interfaces.ExternalServices;
using AuctionSystem.Core.Models.SignalR;

namespace AuctionSystem.API.Services.RealtimeNotification;

public class AuctionRealtimeNotificationService : IAuctionRealtimeNotificationService
{
    private readonly IHubContext<AuctionHub> _auctionHubContext;
    private readonly ILogger<AuctionRealtimeNotificationService> _logger;

    public AuctionRealtimeNotificationService(
        IHubContext<AuctionHub> auctionHubContext,
        ILogger<AuctionRealtimeNotificationService> logger)
    {
        _auctionHubContext = auctionHubContext;
        _logger = logger;
    }

    public async Task BroadcastNewBidAsync(AuctionBidUpdateModel bidUpdate)
    {
        try
        {
            string auctionGroupName = GetAuctionGroupName(bidUpdate.ProductId);

            _logger.LogInformation(
                "Broadcasting new bid to auction group: {AuctionGroup}. ProductId: {ProductId}, BidAmount: {BidAmount}",
                auctionGroupName,
                bidUpdate.ProductId,
                bidUpdate.BidAmount);

            await _auctionHubContext.Clients
                .Group(auctionGroupName)
                .SendAsync("ReceiveNewBidUpdate", bidUpdate);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error broadcasting new bid to auction. ProductId: {ProductId}",
                bidUpdate.ProductId);
            throw;
        }
    }

    public async Task JoinAuctionGroupAsync(string connectionId, int productId)
    {
        try
        {
            string auctionGroupName = GetAuctionGroupName(productId);

            await _auctionHubContext.Groups.AddToGroupAsync(connectionId, auctionGroupName);

            var statusUpdate = new AuctionConnectionStatusModel
            {
                ProductId = productId,
                UserId = 0,
                UserDisplayName = string.Empty,
                StatusChangedUtcTime = DateTime.UtcNow,
                ConnectionStatus = "Connected",
                ActiveWatchersCount = AuctionGroupTracker.GetConnectedUserCount(productId)
            };

            await _auctionHubContext.Clients.Group(auctionGroupName)
                .SendAsync("ReceiveConnectionStatusChange", statusUpdate);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error adding connection to auction group. ProductId: {ProductId}, ConnectionId: {ConnectionId}",
                productId,
                connectionId);
            throw;
        }
    }

    public async Task LeaveAuctionGroupAsync(string connectionId, int productId)
    {
        try
        {
            string auctionGroupName = GetAuctionGroupName(productId);

            await _auctionHubContext.Groups.RemoveFromGroupAsync(connectionId, auctionGroupName);

            var statusUpdate = new AuctionConnectionStatusModel
            {
                ProductId = productId,
                UserId = 0,
                UserDisplayName = string.Empty,
                StatusChangedUtcTime = DateTime.UtcNow,
                ConnectionStatus = "Disconnected",
                ActiveWatchersCount = AuctionGroupTracker.GetConnectedUserCount(productId)
            };

            await _auctionHubContext.Clients.Group(auctionGroupName)
                .SendAsync("ReceiveConnectionStatusChange", statusUpdate);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error removing connection from auction group. ProductId: {ProductId}, ConnectionId: {ConnectionId}",
                productId,
                connectionId);
            throw;
        }
    }

    private static string GetAuctionGroupName(int productId)
    {
        return $"Auction_{productId}";
    }
}
