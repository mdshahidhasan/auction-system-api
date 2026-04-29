using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionSystem.API.Controllers;

[ApiController]
[Authorize(Roles = "admin")]
[Route("users")]
public class UserAdminController : ApiBaseController
{
    private readonly ILogger<UserAdminController> _logger;
    private readonly IUserAdminApp _userAdminApp;

    public UserAdminController(ILogger<UserAdminController> logger, IUserAdminApp userAdminApp)
    {
        _logger = logger;
        _userAdminApp = userAdminApp;
    }

    [HttpGet]
    public async Task<ActionResult<ServiceResult<List<UserReadModel>>>> GetUsers(UserQueryModel queryModel)
    {
        _logger.LogDebug("GetUsers()");

        var result = await _userAdminApp.GetUsers(queryModel);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<ServiceResult<UserReadModel>>> GetUserById([FromRoute] int userId)
    {
        _logger.LogDebug("GetUserById() {userId}", userId);

        var result = await _userAdminApp.GetUserById(userId);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<ServiceResult>> UpdateUser([FromRoute] int userId, [FromBody] UserAdminUpdateModel model)
    {
        _logger.LogDebug("UpdateUser() {userId}, {@model}", userId, model);

        var result = await _userAdminApp.UpdateUserById(userId, model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [HttpDelete("{userId}")]
    public async Task<ActionResult<ServiceResult>> DeleteUser([FromRoute] int userId)
    {
        _logger.LogDebug("DeleteUser() {userId}", userId);

        var result = await _userAdminApp.DeleteUserById(userId);

        return new JsonResult(result) { StatusCode = result.Code };
    }
}