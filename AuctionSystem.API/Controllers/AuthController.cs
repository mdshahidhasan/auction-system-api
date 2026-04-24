using AuctionSystem.Core.Interfaces.Apps;
using AuctionSystem.Core.Models;
using AuctionSystem.Core.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionSystem.API.Controllers;

[ApiController]
[Route("/")]
public class AuthController : ApiBaseController
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthApp _authApp;

    public AuthController(ILogger<AuthController> logger, IAuthApp authApp)
    {
        _logger = logger;
        _authApp = authApp;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ServiceResult<Token>>> Login([FromBody] AuthLoginModel model)
    {
        _logger.LogDebug("Login() {email}", model.Email);

        var result = await _authApp.Login(model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    public async Task<ActionResult<ServiceResult>> VerifyEmail([FromBody] AuthVerifyEmailModel model)
    {
        _logger.LogDebug("VerifyEmail() {email}", model.Email);

        var result = await _authApp.VerifyEmail(model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ServiceResult>> ForgotPassword([FromBody] AuthForgotPasswordModel model)
    {
        _logger.LogDebug("ForgotPassword() {email}", model.Email);

        var result = await _authApp.ForgotPassword(model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<ActionResult<ServiceResult>> ResetPassword([FromBody] AuthResetPasswordModel model)
    {
        _logger.LogDebug("ResetPassword() {model}", model);

        var result = await _authApp.ResetPassword(model);

        return new JsonResult(result) { StatusCode = result.Code };
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ServiceResult>> ChangePassword([FromBody] AuthChangePasswordModel model)
    {
        _logger.LogDebug("ChangePassword() {model}", model);

        var result = await _authApp.ChangePassword(UserId, model);

        return new JsonResult(result) { StatusCode = result.Code };
    }
}