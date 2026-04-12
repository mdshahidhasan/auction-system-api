namespace AuctionSystem.Core.Models;

public class Token
{
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public Token()
    {
    }

    public Token(string value, string type)
    {
        Value = value;
        Type = type;
    }
}