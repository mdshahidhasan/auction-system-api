using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Product;

namespace AuctionSystem.Core.Interfaces.Apps;

public interface IProductApp
{
    Task<ServiceResult<List<ProductReadModel>>> GetProducts();
    Task<ServiceResult<ProductReadModel>> GetProductById(int id);
}