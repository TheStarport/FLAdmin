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
    [HttpGet("isonline")]
    public async Task<IActionResult> IsCharacterOnline([FromQuery] string name, CancellationToken token)
    {
        var ret = await flHookService.CharacterIsOnline(name, token);

        return ret.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    [HttpPatch("kick")]
    public async Task<IActionResult> KickCharacter([FromQuery] string name, CancellationToken token)
    {
        var ret = await flHookService.KickCharacter(name, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpGet("onlineplayers")]
    public async Task<IActionResult> GetOnlinePlayers(CancellationToken token)
    {
        var ret = await flHookService.GetOnlineCharacters(token);
        return ret.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    [HttpPatch("kill")]
    public async Task<IActionResult> KillCharacter([FromQuery] string name, CancellationToken token)
    {
        var ret = await flHookService.KillCharacter(name, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpPatch("messagecharacter")]
    public async Task<IActionResult> MessagePlayer([FromQuery] string name, [FromQuery] string message,
        CancellationToken token)
    {
        var ret = await flHookService.MessagePlayer(name, message, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpPatch("messagesystem")]
    public async Task<IActionResult> MessageSystem([FromQuery] string system, [FromQuery] string message,
        CancellationToken token)
    {
        var ret = await flHookService.MessageSystem(system, message, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpPatch("messageuniverse")]
    public async Task<IActionResult> MessageUniverse([FromQuery] string message, CancellationToken token)
    {
        var ret = await flHookService.MessageUniverse(message, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpPatch("beamcharactertobase")]
    public async Task<IActionResult> BeamPlayer([FromQuery] string player, [FromQuery] string baseName,
        CancellationToken token)
    {
        var ret = await flHookService.BeamPlayerToBase(player, baseName, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpPatch("teleportcharacter")]
    public async Task<IActionResult> TeleportPlayer([FromQuery] string player, [FromQuery] string system,
        [FromQuery] Vector3 pos, CancellationToken token)
    {
        var ret = await flHookService.TeleportPlayerToSpot(system, player, pos, token);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpGet("config")]
    public async Task<IActionResult> GetFlHookConfig(CancellationToken token)
    {
        var ret = await configService.GetFlHookConfig(token);
        return ret.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: Ok);
    }
}