namespace AuctionSystem.API.Models.Product;

public class ProductPhotoUpdateModel
{
    public int ProductId { get; set; }
    public List<IFormFile> NewPhotos { get; set; } = new();
    public List<int> RemovePhotoIds { get; set; } = new();
}