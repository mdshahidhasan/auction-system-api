using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Models;
using System.Security.Claims;

namespace AuctionSystem.Core.Interfaces.SecurityServices;

public interface ITokenService
{
    Token GenerateAuthToken(User user, bool rememberMe);
    string GeneratePasswordResetToken(User user);
    string GenerateEmailVerificationToken(User user);
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
    Dictionary<string, string>? DecodeToken(string token);
}