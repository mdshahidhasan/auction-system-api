using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.Core.Models.User;

public class UserSearchModel
{
    public int Limit { get; set; } = 10;
    public int Offset { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVerified { get; set; }
}
