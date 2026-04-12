namespace AuctionSystem.Core.Models.User;

public class UserReadModel
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public string? Location { get; set; }
    public string? PhotoPath { get; set; }
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
}
