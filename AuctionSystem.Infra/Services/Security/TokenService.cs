using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Interfaces.SecurityServices;
using AuctionSystem.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuctionSystem.Infra.Services.Security;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Token GenerateAuthToken(User user, bool rememberMe)
    {
        var authMinutes = int.TryParse(_configuration["Jwt:AccessTokenMinutes"], out var minutes) ? minutes : 60;
        var rememberMeDays = int.TryParse(_configuration["Jwt:RememberMeDays"], out var days) ? days : 30;
        var expiresAt = rememberMe ? DateTime.UtcNow.AddDays(rememberMeDays) : DateTime.UtcNow.AddMinutes(authMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Role, user.Role),
            new("token_use", "auth"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var value = GenerateJwt(claims, expiresAt);

        return new Token
        {
            Value = value,
            Type = "Bearer"
        };
    }

    public string GeneratePasswordResetToken(User user)
    {
        var resetMinutes = int.TryParse(_configuration["Jwt:PasswordResetTokenMinutes"], out var minutes) ? minutes : 15;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("token_use", "password_reset"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return GenerateJwt(claims, DateTime.UtcNow.AddMinutes(resetMinutes));
    }

    public string GenerateEmailVerificationToken(User user)
    {
        var verifyMinutes = int.TryParse(_configuration["Jwt:EmailVerificationTokenMinutes"], out var minutes) ? minutes : 1440;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("token_use", "email_verification"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        return GenerateJwt(claims, DateTime.UtcNow.AddMinutes(verifyMinutes));
    }

    public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true)
    {
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = validateLifetime,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "AuctionSystem",
                ValidAudience = _configuration["Jwt:Audience"] ?? "AuctionSystem.Client",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSigningKey())),
                ClockSkew = TimeSpan.Zero
            }, out _);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public Dictionary<string, string>? DecodeToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(token))
        {
            return null;
        }

        try
        {
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => g.First().Value);
        }
        catch
        {
            return null;
        }
    }

    private string GenerateJwt(IEnumerable<Claim> claims, DateTime expiresAt)
    {
        var issuer = _configuration["Jwt:Issuer"] ?? "AuctionSystem";
        var audience = _configuration["Jwt:Audience"] ?? "AuctionSystem.Client";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSigningKey()));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GetSigningKey()
    {
        var secret = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("Jwt:Key is not configured.");
        }

        return secret;
    }
}