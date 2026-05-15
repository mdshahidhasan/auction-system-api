using System.Security.Claims;
using System.Security.Principal;

namespace AuctionSystem.Core.Extensions;

public static class IdentityExtensions
{
    public static string? GetUserEmail(this IIdentity? identity)
    {
        return GetClaimValue(identity, ClaimTypes.Email) ?? GetClaimValue(identity, "email");
    }

    public static string? GetUserName(this IIdentity? identity)
    {
        return GetClaimValue(identity, ClaimTypes.Name) ?? identity?.Name;
    }

    public static string? GetUserRole(this IIdentity? identity)
    {
        return GetClaimValue(identity, ClaimTypes.Role) ?? GetClaimValue(identity, "role");
    }

    public static int GetUserId(this IIdentity? identity)
    {
        var value = GetClaimValue(identity, ClaimTypes.NameIdentifier) ?? GetClaimValue(identity, "sub") ?? GetClaimValue(identity, "userId");

        return int.TryParse(value, out var id) ? id : 0;
    }

    private static string? GetClaimValue(IIdentity? identity, string claimType)
    {
        if (identity is not ClaimsIdentity claimsIdentity)
        {
            return null;
        }

        return claimsIdentity.FindFirst(claimType)?.Value;
    }

    /// <summary>
    /// Extension method for ClaimsPrincipal to extract the user ID from JWT claims.
    /// Used for SignalR hub connections and other contexts where we have ClaimsPrincipal.
    /// </summary>
    public static int GetUserIdFromClaims(this ClaimsPrincipal? principal)
    {
        if (principal == null)
        {
            return 0;
        }

        // Try multiple common claim types for user ID
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ??
                         principal.FindFirst("sub") ??
                         principal.FindFirst("userId") ??
                         principal.FindFirst("UserId");

        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return 0;
    }
}