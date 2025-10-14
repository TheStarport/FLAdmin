using FlAdmin.Common.Models.Auth;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/flserver")]
[AdminAuthorize(Role.ManageServer)]
public class FlServerController(FlServerManager server, ConfigService configService, FlHookService flhook) : ControllerBase
{
    // ReSharper disable once StringLiteralTypo
    [HttpPatch("restartserver")]
    public async Task<IActionResult> RestartServer([FromBody] int delay, CancellationToken token)
    {
        if (!server.IsAlive()) return BadRequest("Server is currently not running and thus cant be restarted.");

        if (!server.FlSeverIsReady()) return BadRequest("Server is not currently fully initialized.");

        //We will give an extra 15 seconds before checking of a restart actually happened
        await server.RestartServer(delay);
        await Task.Delay(TimeSpan.FromSeconds(delay + 15));

        if (server.FlSeverIsReady()) return Ok();

        return new ObjectResult(StatusCodes.Status500InternalServerError);
    }

    [HttpPatch("stopserver")]
    public async Task<IActionResult> StopServer(CancellationToken token)
    {
        await server.Terminate(token);
        return Ok("Server Terminated");
    }

    [HttpPatch("startserver")]
    public async Task<IActionResult> StartServer(CancellationToken token)
    {
        var res = server.StartServer(token);
        res.Match(Some:  err => err.ParseError(this), None: () => { } );

        try
        {
            while (token.IsCancellationRequested == false || server.FlserverReady)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }

            return Ok("Server successfully started.");

        }
        catch (OperationCanceledException ex)
        {
            return new ObjectResult(StatusCodes.Status499ClientClosedRequest);
        }
    }
 
    [HttpGet("playercount")]
    public Task<IActionResult> GetPlayerCount()
    {
        return Task.FromResult<IActionResult>(Ok(server.GetCurrentPlayerCount()));
    }

    [HttpGet("memory")]
    public Task<IActionResult> GetServerMemory()
    {
        return Task.FromResult<IActionResult>(Ok(server.GetServerMemory()));
    }
    
    [HttpGet("serveruptime")]
    public Task<IActionResult> GetServerUptime()
    {
        return Task.FromResult<IActionResult>(Ok(server.GetServerUptime()));
    }

    [HttpPatch("setscheduledrestart")]
    public Task<IActionResult> SetScheduledRestart([FromQuery] int delay, [FromQuery] string cronString,
        CancellationToken token)
    {
        var ret = server.SetServerRestart(cronString, delay);
        
        return Task.FromResult(ret.Match<IActionResult>( 
            err => BadRequest(), 
            Ok()));
    }

    //TODO: Get Server Latency.

    //TODO: Update JSON Configs.

    //Gets a json config on a specified path
    [HttpGet("jsonconfig")]
    public async Task<IActionResult> GetJsonConfig([FromQuery] string path, CancellationToken token)
    {
        var ret = await configService.GetJsonConfig(path, token);

        return ret.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: Ok);
    }


    [HttpGet("flserverdiagnostics")]
    public Task<IActionResult> GetFlServerDiagnosticHistory()
    {
        return Task.FromResult<IActionResult>(Ok(server._pastServerDiagnostics.queue));
    }
}