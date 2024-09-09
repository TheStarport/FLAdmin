using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController(IAuthService authService, IAccountService accountService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel login)
    {
        var token = await authService.Authenticate(login.Username, login.Password);
        if (token is null) return Unauthorized("Invalid username or password");
        return Ok(token);
    }

    [HttpPost("setup")]
    [AllowAnonymous]
    public async Task<IActionResult> Setup(string password)
    {
        const string username = "SuperAdmin";

        var login = new LoginModel
        {
            Username = username,
            Password = password
        };

        if (!await accountService.CreateWebMaster(login)) return BadRequest("Setup has already been executed.");

        return Ok("Super Admin successfully created.");
    }

    [Authorize]
    [HttpGet]
    public IActionResult IsAuthenticated()
    {
        return Ok();
    }
}