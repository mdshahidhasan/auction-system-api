namespace AuctionSystem.Core.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public string? Location { get; set; }
    public string? PhotoPath { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;
}