using System.Numerics;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/flhook")]
[AdminAuthorize(Role.ManageServer)]
public class FlHookController(IFlHookService flHookService, ConfigService configService) : ControllerBase
{
    /// <summary>
    /// Checks if a provided character name is currently logged into the server.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="token"></param>
    /// <returns>Bool(True: Is Online, False: Is Offline)</returns>
    [HttpGet("isonline")]
    public async Task<IActionResult> IsCharacterOnline([FromQuery] string name, CancellationToken token)
    {
        var ret = await flHookService.CharacterIsOnline(name, token);

        return ret.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    /// <summary>
    /// Kicks a character by a specified name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("kick")]
    public async Task<IActionResult> KickCharacter([FromQuery] string name, CancellationToken token)
    {
        var ret = await flHookService.KickCharacter(name, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }


    /// <summary>
    /// Gets a list of characters currently online and logged into the server. 
    /// </summary>
    /// <param name="token"></param>
    /// <returns>List of online characters.</returns>
    [HttpGet("onlineplayers")]
    public async Task<IActionResult> GetOnlinePlayers(CancellationToken token)
    {
        var ret = await flHookService.GetOnlineCharacters(token);
        return ret.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    /// <summary>
    /// Force kills a character's ship if they are in space in the server. 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("kill")]
    public async Task<IActionResult> KillCharacter([FromQuery] string name, CancellationToken token)
    {
        var ret = await flHookService.KillCharacter(name, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    /// <summary>
    /// Privately messages a character with a message.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="message"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("messagecharacter")]
    public async Task<IActionResult> MessagePlayer([FromQuery] string name, [FromQuery] string message,
        CancellationToken token)
    {
        var ret = await flHookService.MessagePlayer(name, message, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    /// <summary>
    /// Messages a provided system that all players in that system will see.
    /// </summary>
    /// <param name="system"></param>
    /// <param name="message"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("messagesystem")]
    public async Task<IActionResult> MessageSystem([FromQuery] string system, [FromQuery] string message,
        CancellationToken token)
    {
        var ret = await flHookService.MessageSystem(system, message, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    /// <summary>
    /// Messages every person currently logged into the server and in-game.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("messageuniverse")]
    public async Task<IActionResult> MessageUniverse([FromQuery] string message, CancellationToken token)
    {
        var ret = await flHookService.MessageUniverse(message, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    
    /// <summary>
    /// Beams a character to a specified base.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="baseName"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("beamcharactertobase")]
    public async Task<IActionResult> BeamPlayer([FromQuery] string player, [FromQuery] string baseName,
        CancellationToken token)
    {
        var ret = await flHookService.BeamPlayerToBase(player, baseName, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    /// <summary>
    /// Telports a character to a position and system provided.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="system"></param>
    /// <param name="pos"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("teleportcharacter")]
    public async Task<IActionResult> TeleportPlayer([FromQuery] string player, [FromQuery] string system,
        [FromQuery] Vector3 pos, CancellationToken token)
    {
        var ret = await flHookService.TeleportPlayerToSpot(system, player, pos, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    /// <summary>
    /// Gets FLHook's configuration file.
    /// </summary>
    /// <param name="token"></param>
    /// <returns>FLhook's config file in JSON.</returns>
    [HttpGet("config")]
    public async Task<IActionResult> GetFlHookConfig(CancellationToken token)
    {
        var ret = await configService.GetFlHookConfig(token);
        return ret.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: Ok);
    }
}