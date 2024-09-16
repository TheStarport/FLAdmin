﻿using FlAdmin.Common.Extensions;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController(IAuthService authService, IAccountService accountService, ILogger<AuthenticationController> logger) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModel login)
    {
        var token = await authService.Authenticate(login.Username, login.Password);
        return  token.Match<IActionResult>(
            Some: response => Ok(response),
            None: () => Unauthorized("Invalid username or password.")
            );
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
        var res = await accountService.CreateWebMaster(login);
        return res.Match<IActionResult>(
            Some: err => err.ParseAccountError(this),
            None: Ok("SuperAdmin successfully created.")
        );
    }

    [Authorize]
    [HttpGet]
    public IActionResult IsAuthenticated()
    {
        return Ok();
    }
}