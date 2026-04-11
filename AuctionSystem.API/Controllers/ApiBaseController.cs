using AuctionSystem.Core.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace AuctionSystem.API.Controllers;

[ApiController]
public abstract class ApiBaseController : ControllerBase
{
    protected string? UserEmail => User?.Identity?.GetUserEmail();

    protected string? UserName => User?.Identity?.GetUserName();

    protected string? UserRole => User?.Identity?.GetUserRole();

    protected int UserId => User?.Identity?.GetUserId() ?? 0;
}