namespace AuctionSystem.Core.Models.Auth;

public class AuthVerifyEmailModel
{
    public string Email { get; set; } = string.Empty;
    public string VerificationToken { get; set; } = string.Empty;
}