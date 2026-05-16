using System.Net.Http.Headers;
using System.Text;
using AuctionSystem.Core.Interfaces.ExternalServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuctionSystem.Infra.Services.External;

public sealed class MailgunEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MailgunEmailService> _logger;

    public MailgunEmailService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MailgunEmailService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, string? textBody = null)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            throw new ArgumentException("Recipient email address is required.", nameof(to));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Email subject is required.", nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(htmlBody) && string.IsNullOrWhiteSpace(textBody))
        {
            throw new ArgumentException("At least one email body must be provided.");
        }

        string apiKey = GetRequiredSetting("Mailgun:ApiKey");
        string domain = GetRequiredSetting("Mailgun:Domain");
        string sender = GetRequiredSetting("Mailgun:Sender");

        var fields = new List<KeyValuePair<string, string>>
        {
            new("from", sender),
            new("to", to),
            new("subject", subject)
        };

        if (!string.IsNullOrWhiteSpace(htmlBody))
        {
            fields.Add(new KeyValuePair<string, string>("html", htmlBody));
        }

        if (!string.IsNullOrWhiteSpace(textBody))
        {
            fields.Add(new KeyValuePair<string, string>("text", textBody));
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{domain}/messages")
        {
            Content = new FormUrlEncodedContent(fields)
        };

        string basicAuthToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{apiKey}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuthToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email sent successfully to {Recipient} using Mailgun.", to);
            return;
        }

        string responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogError(
            "Mailgun email send failed. StatusCode: {StatusCode}. Recipient: {Recipient}. Response: {ResponseBody}",
            (int)response.StatusCode,
            to,
            responseBody);

        throw new InvalidOperationException(
            $"Mailgun email send failed with status code {(int)response.StatusCode} ({response.ReasonPhrase}).");
    }

    private string GetRequiredSetting(string key)
    {
        string? value = _configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Configuration value '{key}' is missing.");
        }

        return value;
    }
}