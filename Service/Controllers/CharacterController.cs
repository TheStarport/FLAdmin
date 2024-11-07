using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Controllers;

//TODO:  Character Model conversion to convert things such as hashes to more human readable data like nicknames.
[ApiController]
[Route("api/characters")]
[AdminAuthorize(Role.ManageAccounts)]
public class CharacterController(ICharacterService characterService) : ControllerBase
{
    [HttpGet("charactername")]
    public async Task<IActionResult> GetCharacterByName([FromQuery] string name)
    {
        var character = await characterService.GetCharacterByName(name);

        return character.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    [HttpGet("charactersofaccount")]
    public async Task<IActionResult> GetCharactersOfAccount([FromQuery] string accountId)
    {
        var characters = await characterService.GetCharactersOfAccount(accountId);

        return characters.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    [HttpPatch("edit")]
    public async Task<IActionResult> EditCharacter([FromBody] Character character)
    {
        var res = await characterService.UpdateCharacter(character);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Character successfully edited.")
        );
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteCharacter([FromQuery] string name)
    {
        var res = await characterService.DeleteCharacter(name);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Character successfully deleted.")
        );
    }

    [HttpPatch("update")]
    public async Task<IActionResult> MoveCharacter([FromQuery] string characterName, [FromQuery] string newAccountId)
    {
        var res = await characterService.MoveCharacter(characterName, newAccountId);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"{characterName} successfully moved to {newAccountId}.")
        );
    }

    [HttpDelete("removeallfromaccount")]
    public async Task<IActionResult> DeleteCharactersFromAccount([FromQuery] string accountId)
    {
        var res = await characterService.DeleteAllCharactersOnAccount(accountId);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Characters on account {accountId} deleted.")
        );
    }

    [HttpPut("add")]
    public async Task<IActionResult> AddCharacter([FromBody] Character character)
    {
        var res = await characterService.AddCharacter(character);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Character {character.CharacterName} added.")
        );
    }
}