using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("user/{userId}/products")]
public class UserProductController : ApiBaseController
{
    private readonly ILogger<UserProductController> _logger;
    private readonly IUserProductApp _userProductApp;

    public UserProductController(ILogger<UserProductController> logger, IUserProductApp userProductApp)
    {
        _logger = logger;
        _userProductApp = userProductApp;
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResult<ProductReadModel>>> CreateProduct([FromForm] ProductWriteModel model)
    {
        _logger.LogDebug("CreateProduct() {@model}", model);

        var result = await _userProductApp.CreateProduct(UserId, model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [HttpGet]
    public async Task<ActionResult<ServiceResult<List<ProductReadModel>>>> GetUserProducts()
    {
        _logger.LogDebug("GetUserProducts()");

        var result = await _userProductApp.GetUserProducts(UserId);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [HttpPut("{productId}")]
    public async Task<ActionResult<ServiceResult>> UpdateProduct([FromRoute] int userId, [FromRoute] int productId, [FromForm] ProductUpdateModel model)
    {
        _logger.LogDebug("UpdateProduct() {id}, {@model}", productId, model);

        var result = await _userProductApp.UpdateProduct(UserId, userId, productId, model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [HttpDelete("{productId}")]
    public async Task<ActionResult<ServiceResult>> DeleteProduct([FromRoute] int userId, [FromRoute] int productId)
    {
        _logger.LogDebug("DeleteProduct() {id}", productId);

        var result = await _userProductApp.DeleteProduct(UserId, userId, productId);

        return new JsonResult(result) { StatusCode = result.Code };
    }
}