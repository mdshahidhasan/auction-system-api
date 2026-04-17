using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.Core.Models.User;

public class UserQueryModel
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }
}
