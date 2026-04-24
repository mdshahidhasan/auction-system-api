using AuctionSystem.Core.Entities;
using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionSystem.API.Controllers;

[ApiController]
[Route("users")]
public class UserController : ApiBaseController
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserApp _userApp;

    public UserController(ILogger<UserController> logger, IUserApp userApp)
    {
        _logger = logger;
        _userApp = userApp;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<ServiceResult<UserReadModel>>> CreateUser([FromBody] UserWriteModel model)
    {
        _logger.LogDebug("CreateUser() {email}", model.Email);

        var result = await _userApp.CreateUser(model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public async Task<ActionResult<ServiceResult<List<UserReadModel>>>> GetUsers(UserQueryModel queryModel)
    {
        _logger.LogDebug("GetUsers()");

        var result = await _userApp.GetUsers(queryModel);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [Authorize]
    [HttpGet("{userId}")]
    public async Task<ActionResult<ServiceResult<UserReadModel>>> GetUserById([FromRoute] int userId)
    {
        _logger.LogDebug("GetUserById() {userId}", userId);

        var result = await _userApp.GetUserById(UserId, userId, UserRole);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [Authorize]
    [HttpPut("{userId}")]
    public async Task<ActionResult<ServiceResult>> UpdateUser([FromRoute] int userId, [FromForm] UserUpdateModel model)
    {
        _logger.LogDebug("UpdateUser() {userId}, {@model}", userId, model);

        var result = await _userApp.UpdateUser(UserId, userId, model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [Authorize]
    [HttpDelete("{userId}")]
    public async Task<ActionResult<ServiceResult>> DeleteUser([FromRoute] int userId)
    {
        _logger.LogDebug("DeleteUser() {userId}", userId);

        var result = await _userApp.DeleteUser(UserId, userId);

        return new JsonResult(result) { StatusCode = result.Code };
    }
}
