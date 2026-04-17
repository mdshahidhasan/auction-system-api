using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Bid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionSystem.API.Controllers;


[Authorize]
[ApiController]
[Route("product/{productId}")]
public class BidController : ApiBaseController
{
    private readonly ILogger<BidController> _logger;
    private readonly IBidApp _bidApp;

    public BidController(ILogger<BidController> logger, IBidApp bidApp)
    {
        _logger = logger;
        _bidApp = bidApp;
    }

    [HttpPost("bids")]
    public async Task<ActionResult<ServiceResult<BidReadModel>>> CreateBid([FromRoute] int productId, [FromForm] BidWriteModel model)
    {
        _logger.LogDebug("CreateBid() {@model}", model);

        var result = await _bidApp.CreateBid(UserId, productId, model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [HttpGet("public-bids")]
    public async Task<ActionResult<ServiceResult<List<BidPublicReadModel>>>> GetPublicBids([FromRoute] int productId, [FromQuery] BidQueryModel queryModel)
    {
        _logger.LogDebug("GetPublicBids()");

        var result = await _bidApp.GetPublicBids(productId, queryModel);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [Authorize]
    [HttpGet("bids")]
    public async Task<ActionResult<ServiceResult<List<BidReadModel>>>> GetBids([FromRoute] int productId, [FromQuery] BidQueryModel queryModel)
    {
        _logger.LogDebug("GetBids()");

        var result = await _bidApp.GetBids(productId, UserId, UserRole, queryModel);

        return new JsonResult(result) { StatusCode = result.Code };
    }
}
