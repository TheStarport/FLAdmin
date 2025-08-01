using FlAdmin.Common.Models.Auth;
using FlAdmin.Logic.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/flserver")]
[AdminAuthorize(Role.ManageServer)]
public class FlServerController(FlServerManager server) : ControllerBase
{
    // ReSharper disable once StringLiteralTypo
    [HttpPatch("restartserver")]
    public async Task<IActionResult> RestartServer([FromBody] int delay)
    {
        if (!server.IsAlive()) return BadRequest("Server is currently not running and thus cant be restarted.");

        if (!server.FlSeverIsReady()) return BadRequest("Server is not currently fully initialized.");

        //We will give an extra 15 seconds before checking of a restart actually happened
        await Task.WhenAny(server.RestartServer(delay), Task.Delay(TimeSpan.FromSeconds(delay + 15)));

        if (server.FlSeverIsReady()) return Ok();

        return new ObjectResult(StatusCodes.Status500InternalServerError);
    }

    

    //TODO: Get Server Memory & Online Player Counts.
    
    //TODO: Get Server Uptime
    
    //TODO: 
    
}