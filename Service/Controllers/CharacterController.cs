using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FlAdmin.Service.Controllers;

//TODO:  Character Model conversion to convert things such as hashes to more human readable data like nicknames.
[ApiController]
[Route("api/characters")]
[AdminAuthorize(Role.ManageAccounts)]
public class CharacterController(ICharacterService characterService, IFlHookService flHookService) : ControllerBase
{
    /// <summary>
    /// Gets a character by their specified in-game name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("charactername")]
    public async Task<IActionResult> GetCharacterByName([FromQuery] string name, CancellationToken token)
    {
        var character = await characterService.GetCharacterByName(name, token);

        return character.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    /// <summary>
    /// Gets all characters currently online and logged into the server.
    /// </summary>
    /// <param name="token"></param>
    /// <returns>List of characters</returns>
    [HttpGet("onlinecharacters")]
    public async Task<IActionResult> GetOnlineCharacters(CancellationToken token)
    {
        var characters = await flHookService.GetOnlineCharacters(token);

        return characters.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    /// <summary>
    /// Gets a list of characters that belong to the specified account id.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="token"></param>
    /// <returns>List of characters.</returns>
    [HttpGet("charactersofaccount")]
    public async Task<IActionResult> GetCharactersOfAccount([FromQuery] string accountId, CancellationToken token)
    {
        var characters = await characterService.GetCharactersOfAccount(accountId, token);

        return characters.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val));
    }

    /// <summary>
    /// Edits a character via find and replace.
    /// </summary>
    /// <param name="character">New character doc that will replace the one on the database</param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("edit")]
    public async Task<IActionResult> EditCharacter([FromBody] Character character, CancellationToken token)
    {
        var res = await characterService.UpdateCharacter(character, token);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Character successfully edited.")
        );
    }

    /// <summary>
    /// Deletes a specified character by its name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteCharacter([FromQuery] string name, CancellationToken token)
    {
        var res = await characterService.DeleteCharacter(name, token);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Character successfully deleted.")
        );
    }

    
    /// <summary>
    /// Moves character to a specified account.
    /// </summary>
    /// <param name="characterName">name of character to be moved.</param>
    /// <param name="newAccountId">Id of the account the character will be moved to.</param>
    /// <param name="token"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Deletes all characters tied to a specified account.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpDelete("removeallfromaccount")]
    public async Task<IActionResult> DeleteCharactersFromAccount([FromQuery] string accountId, CancellationToken token)
    {
        var res = await characterService.DeleteAllCharactersOnAccount(accountId, token);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Characters on account {accountId} deleted.")
        );
    }

    
    /// <summary>
    /// Adds a character to the database.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPut("add")]
    public async Task<IActionResult> AddCharacter([FromBody] Character character, CancellationToken token)
    {
        var res = await characterService.AddCharacter(character, token);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Character {character.CharacterName} added.")
        );
    }

    
    /// <summary>
    /// Renames a character.
    /// </summary>
    /// <param name="oldName"></param>
    /// <param name="newName"></param>
    /// <param name="token"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets a truncated character document by a specified filter with pagination.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="pageSize"></param>
    /// <param name="page"></param>
    /// <param name="token"></param>
    /// <returns>A page of character summaries that pass the filter.</returns>
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