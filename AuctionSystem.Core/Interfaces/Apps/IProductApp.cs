using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Product;

namespace AuctionSystem.Core.Interfaces.Apps;

public interface IProductApp
{
    Task<ServiceResult<List<ProductReadModel>>> GetProducts(ProductQueryModel queryModel);
    Task<ServiceResult<ProductReadModel>> GetProductById(int id);
}