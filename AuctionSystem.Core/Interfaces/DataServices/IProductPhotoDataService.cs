using AuctionSystem.Core.Entities;
using System.Data;

namespace AuctionSystem.Core.Interfaces.Services;

public interface IProductPhotoDataService
{
    Task<ProductPhoto> UploadPhotos(ProductPhoto productPhoto, IDbTransaction? transaction = null);
    Task<List<ProductPhoto>> GetPhotosByProductId(int productId);
    Task DeletePhotos(int photoId);
    Task DeletePhotosByProductId(int productId, IDbTransaction? transaction = null);
}