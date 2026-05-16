namespace AuctionSystem.Core.Interfaces.ExternalServices;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null);
}