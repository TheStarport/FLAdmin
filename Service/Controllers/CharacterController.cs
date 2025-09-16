using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FlAdmin.Service.Controllers;

//TODO:  Character Model conversion to convert things such as hashes to more human readable data like nicknames.
[ApiController]
[Route("api/characters")]
[AdminAuthorize(Role.ManageAccounts)]
public class CharacterController(ICharacterService characterService, IFlHookService flHookService) : ControllerBase
{
    [HttpGet("charactername")]
    public async Task<IActionResult> GetCharacterByName([FromQuery] string name, CancellationToken token)
    {
        var character = await characterService.GetCharacterByName(name, token);

        return character.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    [HttpGet("charactersummary")]
    public async Task<IActionResult> GetCharacterSummary([FromQuery] int pageSize, [FromQuery] int page,
        CancellationToken token)
    {
        return BadRequest("Currently not implemented");
    }

    [HttpGet("onlinecharacters")]
    public async Task<IActionResult> GetOnlineCharacters(CancellationToken token)
    {
        var characters = await flHookService.GetOnlineCharacters(token);

        return characters.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    [HttpGet("charactersofaccount")]
    public async Task<IActionResult> GetCharactersOfAccount([FromQuery] string accountId, CancellationToken token)
    {
        var characters = await characterService.GetCharactersOfAccount(accountId, token);

        return characters.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    [HttpPatch("edit")]
    public async Task<IActionResult> EditCharacter([FromBody] Character character, CancellationToken token)
    {
        var res = await characterService.UpdateCharacter(character, token);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Character successfully edited.")
        );
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteCharacter([FromQuery] string name, CancellationToken token)
    {
        var res = await characterService.DeleteCharacter(name, token);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Character successfully deleted.")
        );
    }

    [HttpPatch("movecharactertoaccount")]
    public async Task<IActionResult> MoveCharacter([FromQuery] string characterName, [FromQuery] string newAccountId,
        CancellationToken token)
    {
        var res = await characterService.MoveCharacter(characterName, newAccountId, token);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"{characterName} successfully moved to {newAccountId}.")
        );
    }

    [HttpDelete("removeallfromaccount")]
    public async Task<IActionResult> DeleteCharactersFromAccount([FromQuery] string accountId, CancellationToken token)
    {
        var res = await characterService.DeleteAllCharactersOnAccount(accountId, token);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Characters on account {accountId} deleted.")
        );
    }

    [HttpPut("add")]
    public async Task<IActionResult> AddCharacter([FromBody] Character character, CancellationToken token)
    {
        var res = await characterService.AddCharacter(character, token);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Character {character.CharacterName} added.")
        );
    }

    [HttpPatch("rename")]
    public async Task<IActionResult> RenameCharacter([FromQuery] string oldName, [FromQuery] string newName,
        CancellationToken token)
    {
        var res = await characterService.RenameCharacter(oldName, newName, token);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"{oldName} successfully renamed to {newName}.")
        );
    }

    [HttpPatch("movetoaccount")]
    public async Task<IActionResult> MoveCharacterToAccount([FromQuery] string characterName,
        [FromQuery] string newAccountId, CancellationToken token)
    {
        var res = await characterService.MoveCharacter(characterName, newAccountId, token);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"{characterName} successfully moved to account {newAccountId}.")
        );
    }

    [HttpGet("summariesbyfilter")]
    public async Task<IActionResult> GetCharacterSummariesByFilter([FromQuery] BsonDocument filter,
        [FromQuery] int pageSize, [FromQuery] int page, CancellationToken token)
    {
        var res = await characterService.GetCharacterSummaries(filter, page, pageSize, token);

        return res.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val)
        );
    }
}