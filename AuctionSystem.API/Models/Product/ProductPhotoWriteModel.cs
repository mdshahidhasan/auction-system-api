namespace AuctionSystem.API.Models.Product;

public class ProductPhotoWriteModel
{
    public int ProductId { get; set; }
    public List<IFormFile> Photos { get; set; } = new();
}