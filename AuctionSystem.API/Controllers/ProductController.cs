using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionSystem.API.Controllers;

[ApiController]
[Route("products")]
public class ProductController : ApiBaseController
{
    private readonly ILogger<ProductController> _logger;
    private readonly IProductApp _productApp;

    public ProductController(ILogger<ProductController> logger, IProductApp productApp)
    {
        _logger = logger;
        _productApp = productApp;
    }

    // Public product browsing endpoints
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ServiceResult<List<ProductReadModel>>>> GetProducts(ProductQueryModel queryModel)
    {
        _logger.LogDebug("GetProducts()");

        var result = await _productApp.GetProducts(queryModel);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [AllowAnonymous]
    [HttpGet("{productId}")]
    public async Task<ActionResult<ServiceResult<ProductReadModel>>> GetProductById(int productId)
    {
        _logger.LogDebug("GetProductById() {productId}", productId);

        var result = await _productApp.GetProductById(productId);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    // User product management endpoints
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ServiceResult<ProductReadModel>>> CreateProduct([FromForm] ProductWriteModel model)
    {
        _logger.LogDebug("CreateProduct() {@model}", model);

        var result = await _productApp.CreateProduct(UserId, model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [Authorize]
    [HttpGet("~/users/me/products")]
    public async Task<ActionResult<ServiceResult<List<ProductReadModel>>>> GetUserProducts()
    {
        _logger.LogDebug("GetUserProducts()");

        var result = await _productApp.GetUserProducts(UserId);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [Authorize]
    [HttpPut("{productId}")]
    public async Task<ActionResult<ServiceResult>> UpdateProduct([FromRoute] int productId, [FromForm] ProductUpdateModel model)
    {
        _logger.LogDebug("UpdateProduct() {productId}, {@model}", productId, model);

        var result = await _productApp.UpdateProduct(UserId, productId, model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [Authorize]
    [HttpDelete("{productId}")]
    public async Task<ActionResult<ServiceResult>> DeleteProduct([FromRoute] int productId)
    {
        _logger.LogDebug("DeleteProduct() {productId}", productId);

        var result = await _productApp.DeleteProduct(UserId, productId);

        return new JsonResult(result) { StatusCode = result.Code };
    }
}
