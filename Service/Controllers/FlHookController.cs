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
public class FlHookController(IFlHookService flHookService) : ControllerBase
{
    [HttpGet("isonline")]
    public async Task<IActionResult> IsCharacterOnline([FromQuery] string name)
    {
        var ret = await flHookService.CharacterIsOnline(name);

        return ret.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    [HttpPatch("kick")]
    public async Task<IActionResult> KickCharacter([FromQuery] string name)
    {
        var ret = await flHookService.KickCharacter(name);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpPatch("kill")]
    public async Task<IActionResult> KillCharacter([FromQuery] string name)
    {
        var ret = await flHookService.KillCharacter(name);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpPatch("messagecharacter")]
    public async Task<IActionResult> MessagePlayer([FromQuery] string name, [FromQuery] string message)
    {
        var ret = await flHookService.MessagePlayer(name, message);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpPatch("messagesystem")]
    public async Task<IActionResult> MessageSystem([FromQuery] string system, [FromQuery] string message)
    {
        var ret = await flHookService.MessageSystem(system, message);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpPatch("messageuniverse")]
    public async Task<IActionResult> MessageUniverse([FromQuery] string message)
    {
        var ret = await flHookService.MessageUniverse(message);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpPatch("beamcharacter")]
    public async Task<IActionResult> BeamPlayer([FromQuery] string player, [FromQuery] string baseName)
    {
        var ret = await flHookService.BeamPlayerToBase(player, baseName);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }

    [HttpPatch("beamcharacter")]
    public async Task<IActionResult> TeleportPlayer([FromQuery] string player, [FromQuery] string system,
        [FromQuery] Vector3 pos)
    {
        var ret = await flHookService.TeleportPlayerToSpot(player, system, pos);
        return ret.Match<IActionResult>(
            err => err.ParseError(this),
            Ok());
    }
}