using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Product;
using System.Data;

namespace AuctionSystem.Core.Interfaces.Services;

public interface IProductDataService
{
    Task<Product> CreateProduct(Product product, IDbTransaction? transaction = null);
    Task<ServiceResult<List<Product>>> GetProducts(ProductSearchModel searchModel);
    Task<ServiceResult<List<Product>>> GetProductsByUserId(int userId, ProductSearchModel searchModel);
    Task<Product?> GetProductById(int id);
    Task UpdateProduct(int id, ProductUpdateModel productUpdateModel);
    Task UpdateCurrentHighestBid(int id, decimal? currentHighestBid, IDbTransaction? transaction = null);
    Task DeleteProduct(int id, IDbTransaction? transaction = null);
}