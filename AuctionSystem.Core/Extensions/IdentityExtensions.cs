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
}