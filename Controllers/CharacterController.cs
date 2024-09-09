using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FlAdmin.Controllers;

[ApiController]
[Route("api/characters")]
[AdminAuthorize(Role.ManageAccounts)]
public class CharacterController(ICharacterService characterService) : ControllerBase
{
    [HttpGet("charactername")]
    public async Task<IActionResult> GetCharacterByName([FromQuery] string name)
    {
        var character = await characterService.GetCharacterByName(name);
        if (character is null)
            return NotFound("A Character with that name does not exist.");
        return Ok(character);
    }

    [HttpGet("charactersofaccount")]
    public async Task<IActionResult> GetCharactersOfAccount([FromQuery] string accountId)
    {
        var characters = await characterService.GetCharactersOfAccount(accountId);
        if (characters.Count is 0)
        {
            return NotFound("No characters found on that accountId.");
        }

        return Ok(characters);
    }

    [HttpPatch("edit")]
    public async Task<IActionResult> EditCharacter([FromBody] Character character)
    {
        await characterService.UpdateCharacter(character);
        return Ok("Character updated.");
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteCharacter([FromQuery] string name)
    {
        await characterService.DeleteCharacter(name);
        return Ok("Character deleted.");
    }

    [HttpPatch("update")]
    public async Task<IActionResult> MoveCharacter([FromQuery] string characterName, [FromQuery] string newAccountId)
    {
        await characterService.MoveCharacter(characterName, newAccountId);
        return Ok("Character moved.");
    }
    [HttpDelete("removeallfromaccount")]
    public async Task<IActionResult> DeleteCharactersFromAccount([FromQuery] string accountId)
    {
        await characterService.DeleteAllCharactersOnAccount(accountId);
        return Ok($"Characters on account {accountId} deleted.");
    }

    [HttpPut("add")]
    public async Task<IActionResult> AddCharacter([FromBody] Character character)
    {
        await characterService.AddCharacter(character);
        return Ok($"Character {character.CharacterName} added.");
    }
    
    
    
}