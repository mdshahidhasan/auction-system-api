using AutoMapper;
using AuctionSystem.App.Validators;
using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Interfaces.Services;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Product;

namespace AuctionSystem.App.Apps;

public class UserProductApp : IUserProductApp
{
    private readonly IProductDataService _productService;
    private readonly IProductPhotoDataService _productPhotoService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserProductApp(IProductDataService productService, IProductPhotoDataService productPhotoService, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _productService = productService;
        _productPhotoService = productPhotoService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<ProductReadModel>> CreateProduct(int requesterUserId, int routeUserId, ProductWriteModel productWriteModel)
    {
        if (requesterUserId != routeUserId)
        {
            return new ServiceResult<ProductReadModel>
            {
                Code = 403,
                Message = "Forbidden: You do not have permission to create a product for this user."
            };
        }

        var createProductValidator = new CreateProductValidator();
        var validationResult = createProductValidator.Validate(productWriteModel);

        if (!validationResult.IsValid)
        {
            return new ServiceResult<ProductReadModel>
            {
                Code = 400,
                Message = string.Join("; ", validationResult.Errors.Select(error => error.ErrorMessage))
            };
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var product = _mapper.Map<Product>(productWriteModel);
            product.UserId = requesterUserId;
            product.AuctionStarts = productWriteModel.AuctionStarts ?? DateTime.UtcNow;
            product.AuctionStatus = product.AuctionStarts > DateTime.UtcNow ? "Upcoming" : "Active";
            product.MinBidIncrement = productWriteModel.MinBidIncrement ?? 1;

            var createdProduct = await _productService.CreateProduct(product, _unitOfWork.Transaction);

            foreach (var photo in productWriteModel.Photos)
            {
                await _productPhotoService.UploadPhotos(new ProductPhoto
                {
                    ProductId = createdProduct.Id,
                    PhotoOrder = productWriteModel.Photos.IndexOf(photo) + 1,
                    PhotoUrl = photo.FileName
                }, _unitOfWork.Transaction);
            }

            await _unitOfWork.CommitAsync();

            var productReadModel = _mapper.Map<ProductReadModel>(createdProduct);
            productReadModel.PhotoUrls = productWriteModel.Photos.Select(photo => photo.FileName).ToList();

            return new ServiceResult<ProductReadModel>
            {
                Code = 201,
                Message = "Product created successfully.",
                Data = productReadModel
            };
        }
        catch
        {
            await _unitOfWork.RollbackAsync();

            return new ServiceResult<ProductReadModel>
            {
                Code = 400,
                Message = "Product creation failed."
            };
        }
    }

    public async Task<ServiceResult<List<ProductReadModel>>> GetUserProducts(int requesterUserId, int routeUserId)
    {
        if (requesterUserId != routeUserId)
        {
            return new ServiceResult<List<ProductReadModel>>
            {
                Code = 403,
                Message = "Forbidden: You do not have permission to view this user's products."
            };
        }

        try
        {
            var products = await _productService.GetProductsByUserId(requesterUserId, new ProductSearchModel
            {
                Limit = 100,
                Offset = 0
            });

            var userProducts = products.Data ?? new List<Product>();

            var productReadModels = _mapper.Map<List<ProductReadModel>>(userProducts);

            for (var i = 0; i < userProducts.Count; i++)
            {
                var photos = await _productPhotoService.GetPhotosByProductId(userProducts[i].Id);
                productReadModels[i].PhotoUrls = photos.OrderBy(photo => photo.PhotoOrder).Select(photo => photo.PhotoUrl).ToList();
            }

            return new ServiceResult<List<ProductReadModel>>
            {
                Code = 200,
                Message = "User products retrieved successfully.",
                Data = productReadModels,
                TotalCount = products.TotalCount
            };
        }
        catch
        {
            return new ServiceResult<List<ProductReadModel>>
            {
                Code = 400,
                Message = "Failed to retrieve user products."
            };
        }
    }

    public async Task<ServiceResult> UpdateProduct(int requesterUserId, int routeUserId, int productId, ProductUpdateModel productUpdateModel)
    {
        if (requesterUserId != routeUserId)
        {
            return new ServiceResult
            {
                Code = 403,
                Message = "Forbidden: You do not have permission to update this product."
            };
        }

        if (productUpdateModel.AskingPrice.HasValue && productUpdateModel.AskingPrice <= 0 ||
            productUpdateModel.StartingPrice.HasValue && productUpdateModel.StartingPrice <= 0 ||
            productUpdateModel.MinBidIncrement.HasValue && productUpdateModel.MinBidIncrement <= 0)
        {
            return new ServiceResult
            {
                Code = 400,
                Message = "Price values must be greater than 0."
            };
        }

        if (productUpdateModel.AuctionStarts.HasValue && productUpdateModel.AuctionEnds.HasValue &&
            productUpdateModel.AuctionEnds <= productUpdateModel.AuctionStarts)
        {
            return new ServiceResult
            {
                Code = 400,
                Message = "Auction end time must be later than auction start time."
            };
        }

        var existingProduct = await _productService.GetProductById(productId);
        if (existingProduct is null)
        {
            return new ServiceResult
            {
                Code = 404,
                Message = "Product not found."
            };
        }

        if (existingProduct.UserId != requesterUserId)
        {
            return new ServiceResult
            {
                Code = 403,
                Message = "Forbidden: You do not have permission to update this product."
            };
        }

        var effectiveAuctionStart = productUpdateModel.AuctionStarts ?? existingProduct.AuctionStarts;
        var effectiveAuctionEnd = productUpdateModel.AuctionEnds ?? existingProduct.AuctionEnds;
        if (effectiveAuctionEnd <= effectiveAuctionStart)
        {
            return new ServiceResult
            {
                Code = 400,
                Message = "Auction end time must be later than auction start time."
            };
        }

        await _productService.UpdateProduct(productId, productUpdateModel);

        return new ServiceResult
        {
            Code = 200,
            Message = "Product updated successfully."
        };
    }

    public async Task<ServiceResult> DeleteProduct(int requesterUserId, int routeUserId, int productId)
    {
        if (requesterUserId != routeUserId)
        {
            return new ServiceResult
            {
                Code = 403,
                Message = "Forbidden: You do not have permission to delete this product."
            };
        }

        var existingProduct = await _productService.GetProductById(productId);
        if (existingProduct is null)
        {
            return new ServiceResult
            {
                Code = 404,
                Message = "Product not found."
            };
        }

        if (existingProduct.UserId != requesterUserId)
        {
            return new ServiceResult
            {
                Code = 403,
                Message = "Forbidden: You do not have permission to delete this product."
            };
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            await _productPhotoService.DeletePhotosByProductId(productId, _unitOfWork.Transaction);
            await _productService.DeleteProduct(productId, _unitOfWork.Transaction);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();

            return new ServiceResult
            {
                Code = 400,
                Message = "Product deletion failed."
            };
        }

        return new ServiceResult
        {
            Code = 200,
            Message = "Product deleted successfully."
        };
    }
}