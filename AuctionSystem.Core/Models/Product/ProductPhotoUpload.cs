using Microsoft.AspNetCore.Http;

public class ProductPhotoUpload
{
    public IFormFile File { get; set; } = null!;
    public int Order { get; set; }
}