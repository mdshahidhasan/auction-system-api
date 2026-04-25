using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Interfaces.Services;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Product;
using AutoMapper;
using AuctionSystem.Core.Entities;

namespace AuctionSystem.App.Apps;

public class ProductApp : IProductApp
{
    private readonly IProductDataService _productService;
    private readonly IMapper _mapper;

    public ProductApp(IProductDataService productService, IMapper mapper)
    {
        _productService = productService;
        _mapper = mapper;
    }

    public async Task<ServiceResult<List<ProductReadModel>>> GetProducts(ProductQueryModel queryModel)
    {
        ProductSearchModel searchModel = new ProductSearchModel
        {
            AuctionStatus = queryModel.AuctionStatus,
            Limit = queryModel.Limit,
            Offset = (queryModel.Page - 1) * queryModel.Limit
        };

        ServiceResult<List<Product>> products = await _productService.GetProducts(searchModel);
        var productReadModels = _mapper.Map<List<ProductReadModel>>(products.Data ?? new List<Product>());

        return new ServiceResult<List<ProductReadModel>>
        {
            Code = products.Code,
            Message = products.Message,
            Data = productReadModels,
            TotalCount = products.TotalCount
        };
    }

    public async Task<ServiceResult<ProductReadModel>> GetProductById(int id)
    {
        Product? product = await _productService.GetProductById(id);
        if (product is null)
        {
            return new ServiceResult<ProductReadModel>
            {
                Code = 404,
                Message = "Product not found."
            };
        }

        var productReadModel = _mapper.Map<ProductReadModel>(product);

        return new ServiceResult<ProductReadModel>
        {
            Code = 200,
            Message = "Product retrieved successfully.",
            Data = productReadModel
        };
    }
}
