using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AuctionSystem.API.Authentication;

public class SignalRJwtQueryStringTokenEventHandler
{
    public static Task OnMessageReceivedAsync(MessageReceivedContext context)
    {
        try
        {
            var httpContext = context.HttpContext;

            bool isSignalRConnection = httpContext.Request.Path.StartsWithSegments("/hubs");

            if (!isSignalRConnection)
            {
                return Task.CompletedTask;
            }

            string? tokenFromQueryString = httpContext.Request.Query["access_token"];

            // If we found a token in the query string, set it for authentication
            if (!string.IsNullOrWhiteSpace(tokenFromQueryString))
            {
                // Assign the token to the context so it can be validated by JWT bearer middleware
                context.Token = tokenFromQueryString;

                httpContext.Request.HttpContext.Items["TokenSource"] = "QueryString";
            }

            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            // If there's any error extracting the token, we let authentication fail gracefully
            // The JWT bearer middleware will handle the validation error
            Console.WriteLine($"Error in OnMessageReceivedAsync: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
