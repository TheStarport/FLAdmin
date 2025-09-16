using FlAdmin.Common.Configs;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Services;
using FlAdmin.Service.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController(
    IAuthService authService,
    IAccountService accountService,
    ILogger<AuthenticationController> logger,
    FlAdminConfig flconfig) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel login, CancellationToken cancellationToken)
    {
        var token = await authService.Authenticate( login.Username, login.Password, cancellationToken);
        return token.Match<IActionResult>(
            response => Ok(response),
            () => Unauthorized("Invalid username or password.")
        );
    }

    [HttpPost("setup")]
    [AllowAnonymous]
    public async Task<IActionResult> Setup(string password, CancellationToken token)
    {
        var username = flconfig.SuperAdminName;

        var login = new LoginModel
        {
            Username = username,
            Password = password
        };

        var res = await accountService.CreateWebMaster(token, login);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("SuperAdmin successfully created.")
        );
    }

    [Authorize]
    [HttpGet]
    public IActionResult IsAuthenticated()
    {
        return Ok();
    }
}