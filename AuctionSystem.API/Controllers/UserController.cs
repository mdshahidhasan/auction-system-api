using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionSystem.API.Controllers;

[ApiController]
[Route("users/me")]
public class UserController : ApiBaseController
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserApp _userApp;

    public UserController(ILogger<UserController> logger, IUserApp userApp)
    {
        _logger = logger;
        _userApp = userApp;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ServiceResult<UserReadModel>>> GetUserById()
    {
        _logger.LogDebug("GetUserById() {userId}", UserId);

        var result = await _userApp.GetUserById(UserId);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [Authorize]
    [HttpPut]
    public async Task<ActionResult<ServiceResult>> UpdateUser([FromForm] UserUpdateModel model)
    {
        _logger.LogDebug("UpdateUser() {@model}", model);

        var result = await _userApp.UpdateUser(UserId, model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [Authorize]
    [HttpDelete]
    public async Task<ActionResult<ServiceResult>> DeleteUser()
    {
        _logger.LogDebug("DeleteUser() {UserId}", UserId);

        var result = await _userApp.DeleteUser(UserId);

        return new JsonResult(result) { StatusCode = result.Code };
    }
}
