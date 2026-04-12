namespace AuctionSystem.Core.Models.Auth;

public class AuthLoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
    public string? TurnstileToken { get; set; }
}