using FlAdmin.Common.Models.Auth;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/flserver")]
[AdminAuthorize(Role.ManageServer)]
public class FlServerController(FlServerManager server, ConfigService configService) : ControllerBase
{
    // ReSharper disable once StringLiteralTypo
    [HttpPatch("restartserver")]
    public async Task<IActionResult> RestartServer([FromBody] int delay)
    {
        if (!server.IsAlive()) return BadRequest("Server is currently not running and thus cant be restarted.");

        if (!server.FlSeverIsReady()) return BadRequest("Server is not currently fully initialized.");

        //We will give an extra 15 seconds before checking of a restart actually happened
        await server.RestartServer(delay);
        await Task.Delay(TimeSpan.FromSeconds(delay + 15));

        if (server.FlSeverIsReady()) return Ok();

        return new ObjectResult(StatusCodes.Status500InternalServerError);
    }
    
    //TODO: Get Server Memory & Online Player Counts.
    
    //TODO: Get Server Uptime
    
    //TODO: Get Server Latency
    
    //TODO: Update JSON Configs.
    
    //Gets a json config on a specified path
    [HttpGet("jsonconfig")]
    public async Task<IActionResult> GetJsonConfig([FromQuery] string path)
    {
      var ret  = await configService.GetJsonConfig(path);
      
      return ret.Match<IActionResult>(
          Left: err => err.ParseError(this),
          Right: Ok);
    }


    [HttpGet("flhookconfig")]
    public async Task<IActionResult> GetFlHookConfig()
    {
        var ret = await configService.GetFlHookConfig();    
        return ret.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: Ok);
    }
    
}