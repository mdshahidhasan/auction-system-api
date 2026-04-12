using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Product;

namespace AuctionSystem.Core.Interfaces.Apps;

public interface IUserProductApp
{
    Task<ServiceResult<ProductReadModel>> CreateProduct(int requesterUserId, ProductWriteModel productWriteModel);
    Task<ServiceResult<List<ProductReadModel>>> GetUserProducts(int requesterUserId);
    Task<ServiceResult> UpdateProduct(int requesterUserId, int routeUserId, int productId, ProductUpdateModel productUpdateModel);
    Task<ServiceResult> DeleteProduct(int requesterUserId, int routeUserId, int productId);
}