using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using AuctionSystem.API.Helpers;
using AuctionSystem.Core.Extensions;
using AuctionSystem.Core.Interfaces.ExternalServices;
using AuctionSystem.Core.Models.SignalR;

namespace AuctionSystem.API.Hubs;

[Authorize]
public class AuctionHub : Hub
{
    private readonly IAuctionRealtimeNotificationService _realtimeNotificationService;

    private readonly ILogger<AuctionHub> _logger;

    public AuctionHub(
        IAuctionRealtimeNotificationService realtimeNotificationService,
        ILogger<AuctionHub> logger)
    {
        _realtimeNotificationService = realtimeNotificationService;
        _logger = logger;
    }


    public override async Task OnConnectedAsync()
    {
        try
        {
            int userId = Context.User?.GetUserIdFromClaims() ?? 0;

            _logger.LogInformation(
                "User connected to AuctionHub. UserId: {UserId}, ConnectionId: {ConnectionId}",
                userId,
                Context.ConnectionId);

            await base.OnConnectedAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error in AuctionHub.OnConnectedAsync(). ConnectionId: {ConnectionId}",
                Context.ConnectionId);
            throw;
        }
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            int userId = Context.User?.GetUserIdFromClaims() ?? 0;

            if (exception != null)
            {
                _logger.LogWarning(
                    exception,
                    "User disconnected from AuctionHub with exception. UserId: {UserId}, ConnectionId: {ConnectionId}",
                    userId,
                    Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation(
                    "User disconnected from AuctionHub. UserId: {UserId}, ConnectionId: {ConnectionId}",
                    userId,
                    Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception disconnectException)
        {
            _logger.LogError(
                disconnectException,
                "Error in AuctionHub.OnDisconnectedAsync(). ConnectionId: {ConnectionId}",
                Context.ConnectionId);
            throw;
        }
    }

    public async Task JoinAuctionGroupAsync(int productId)
    {
        try
        {
            // Extract user information from JWT claims
            int userId = Context.User?.GetUserIdFromClaims() ?? 0;
            string? userFirstName = Context.User?.FindFirst("given_name")?.Value;
            string? userLastName = Context.User?.FindFirst("family_name")?.Value;

            // Validate that we have required user information
            if (userId <= 0)
            {
                _logger.LogWarning(
                    "JoinAuctionGroupAsync called with invalid UserId. ConnectionId: {ConnectionId}",
                    Context.ConnectionId);
                await Clients.Caller.SendAsync(
                    "ReceiveErrorNotification",
                    new
                    {
                        ProductId = productId,
                        ErrorCode = 401,
                        ErrorMessage = "User authentication failed. Please log in again.",
                        ErrorOccurredUtcTime = DateTime.UtcNow
                    });
                return;
            }

            // Create the group name for this auction
            string auctionGroupName = GetAuctionGroupName(productId);

            // Update the internal tracking of connected users
            AuctionGroupTracker.AddUserToAuction(productId, userId);

            _logger.LogInformation(
                "User joined auction group. UserId: {UserId}, ProductId: {ProductId}, ConnectionId: {ConnectionId}",
                userId,
                productId,
                Context.ConnectionId);

            // Delegate group membership and broadcasting to the realtime notification service
            await _realtimeNotificationService.JoinAuctionGroupAsync(Context.ConnectionId, productId);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error in JoinAuctionGroupAsync. ProductId: {ProductId}, ConnectionId: {ConnectionId}",
                productId,
                Context.ConnectionId);

            // Notify the client of the error
            await Clients.Caller.SendAsync(
                "ReceiveErrorNotification",
                new
                {
                    ProductId = productId,
                    ErrorCode = 500,
                    ErrorMessage = "Failed to join auction. Please try again.",
                    ErrorOccurredUtcTime = DateTime.UtcNow
                });
        }
    }

    public async Task LeaveAuctionGroupAsync(int productId)
    {
        try
        {
            int userId = Context.User?.GetUserIdFromClaims() ?? 0;
            string? userFirstName = Context.User?.FindFirst("given_name")?.Value;
            string? userLastName = Context.User?.FindFirst("family_name")?.Value;

            if (userId <= 0)
            {
                _logger.LogWarning(
                    "LeaveAuctionGroupAsync called with invalid UserId. ConnectionId: {ConnectionId}",
                    Context.ConnectionId);
                return;
            }

            string auctionGroupName = GetAuctionGroupName(productId);

            // Update the internal tracking of connected users
            AuctionGroupTracker.RemoveUserFromAuction(productId, userId);

            _logger.LogInformation(
                "User left auction group. UserId: {UserId}, ProductId: {ProductId}, ConnectionId: {ConnectionId}",
                userId,
                productId,
                Context.ConnectionId);

            // Delegate group membership removal and broadcasting to the realtime notification service
            await _realtimeNotificationService.LeaveAuctionGroupAsync(Context.ConnectionId, productId);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Error in LeaveAuctionGroupAsync. ProductId: {ProductId}, ConnectionId: {ConnectionId}",
                productId,
                Context.ConnectionId);
        }
    }

    private static string GetAuctionGroupName(int productId)
    {
        return $"Auction_{productId}";
    }

}
