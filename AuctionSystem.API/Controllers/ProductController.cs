using AuctionSystem.Core.Entities;
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

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ServiceResult<List<ProductReadModel>>>> GetProducts(ProductQueryModel queryModel)
    {
        _logger.LogDebug("GetProducts()");

        var result = await _productApp.GetProducts(queryModel);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResult<ProductReadModel>>> GetProductById(int id)
    {
        _logger.LogDebug("GetProductById() {id}", id);

        var result = await _productApp.GetProductById(id);

        return new JsonResult(result) { StatusCode = result.Code };
    }
}
