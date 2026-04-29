using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Product;

namespace AuctionSystem.Core.Interfaces.Apps;

public interface IProductApp
{
    // Public product browsing
    Task<ServiceResult<List<ProductReadModel>>> GetProducts(ProductQueryModel queryModel);
    Task<ServiceResult<ProductReadModel>> GetProductById(int id);

    // User-specific product management
    Task<ServiceResult<ProductReadModel>> CreateProduct(int requesterUserId, ProductWriteModel productWriteModel);
    Task<ServiceResult<List<ProductReadModel>>> GetUserProducts(int requesterUserId);
    Task<ServiceResult> UpdateProduct(int requesterUserId, int productId, ProductUpdateModel productUpdateModel);
    Task<ServiceResult> DeleteProduct(int requesterUserId, int productId);
}