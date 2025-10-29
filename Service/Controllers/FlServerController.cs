using FlAdmin.Common.Models.Auth;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/flserver")]
[AdminAuthorize(Role.ManageServer)]
public class FlServerController(FlServerManager server, ConfigService configService, FlHookService flhook)
    : ControllerBase
{
    /// <summary>
    /// Restarts the server
    /// </summary>
    /// <param name="delay">delay in seconds of how long FLAdmin should wait before attempting to relaunch the server after killing it.</param>
    /// <param name="token"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Forces the server to shutdown.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("stopserver")]
    public async Task<IActionResult> StopServer(CancellationToken token)
    {
        await server.Terminate(token);
        return Ok("Server Terminated");
    }

    /// <summary>
    /// Tells FLAdmin to start the server. 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("startserver")]
    public async Task<IActionResult> StartServer(CancellationToken token)
    {
        var res = server.StartServer(token);
        res.Match(Some: err => err.ParseError(this), None: () => { });

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

    /// <summary>
    /// Gets count of currently logged on players.
    /// </summary>
    /// <returns></returns>
    [HttpGet("playercount")]
    public Task<IActionResult> GetPlayerCount()
    {
        return Task.FromResult<IActionResult>(Ok(server.GetCurrentPlayerCount()));
    }

    /// <summary>
    /// Gets a list of server memory usage every minute over 6 hours.
    /// </summary>
    /// <returns></returns>
    [HttpGet("memory")]
    public Task<IActionResult> GetServerMemory()
    {
        return Task.FromResult<IActionResult>(Ok(server.GetServerMemory()));
    }

    /// <summary>
    /// How long the server has been currently running and up.
    /// </summary>
    /// <returns></returns>
    [HttpGet("serveruptime")]
    public Task<IActionResult> GetServerUptime()
    {
        return Task.FromResult<IActionResult>(Ok(server.GetServerUptime()));
    }

    /// <summary>
    /// Sets a scheduled server restart 
    /// </summary>
    /// <param name="delay">how long the server should wait before relaunching after killing for its scheduled reset.</param>
    /// <param name="cronString">Chron strong representing the periodic time to restart the server.</param>
    /// <param name="token"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets a Json at a specified path. 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("jsonconfig")]
    public async Task<IActionResult> GetJsonConfig([FromQuery] string path, CancellationToken token)
    {
        var ret = await configService.GetJsonConfig(path, token);

        return ret.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: Ok);
    }

    /// <summary>
    /// Gets the server diagnostic data including memory usage, player count, and timestamps
    /// </summary>
    /// <returns></returns>
    [HttpGet("flserverdiagnostics")]
    public Task<IActionResult> GetFlServerDiagnosticHistory()
    {
        return Task.FromResult<IActionResult>(Ok(server._pastServerDiagnostics.queue));
    }
}