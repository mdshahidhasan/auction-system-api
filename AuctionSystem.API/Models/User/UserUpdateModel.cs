namespace AuctionSystem.API.Models.User;

public class UserUpdateModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ContactNumber { get; set; }
    public string? Location { get; set; }
    public IFormFile? Photo { get; set; }
}
