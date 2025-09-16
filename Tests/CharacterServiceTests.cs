using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services.Database;
using FlAdmin.Tests.DataAccessMocks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlAdmin.Tests;

public class CharacterServiceTests
{
    private readonly CharacterService _characterService;

    private CancellationToken _token = CancellationToken.None;
    
    
    public CharacterServiceTests()
    {
        ICharacterDataAccess characterDataAccess = new CharacterDataAccessMock();
        IAccountDataAccess accountDataAccess = new AccountDataAccessMock();
        IValidationService validationService = new ValidationServiceMock();
        IFlHookService flHookService = new FLHookServiceMock();


        _characterService = new CharacterService(characterDataAccess, accountDataAccess, validationService,
            flHookService,
            new FlAdminConfig(), new NullLogger<CharacterService>());
    }

    //Simple test to see mocks are working correctly. 
    [Fact]
    public async Task Should_Get_Character()
    {
        var result = await _characterService.GetCharacterByName("Chad_Games", _token);

        result.IsRight.Should().BeTrue();
    }

    [Fact]
    public async Task When_Removing_Characters_From_Account_Should_Not_Error()
    {
        var result = await _characterService.DeleteAllCharactersOnAccount("123abc456", _token);

        result.IsNone.Should().BeTrue();
    }


    [Fact]
    public async Task When_Getting_Characters_From_Account_Should_Not_Error()
    {
        var result = await _characterService.GetCharactersOfAccount("123abc456", _token);

        result.Match(
            Left: _ => false,
            Right: x => x.Count == 2
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Getting_Characters_From_Nonexistent_Account_Should_Return_Account_Not_Found()
    {
        var result = await _characterService.GetCharactersOfAccount("123", _token);

        result.Match(
            Left: err => err == FLAdminError.AccountNotFound,
            Right: _ => false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Getting_Characters_From_Account_Without_Characters_Should_Return_Character_Not_Found()
    {
        var result = await _characterService.GetCharactersOfAccount("abc123456", _token);

        result.Match(
            Left: err => err == FLAdminError.CharacterNotFound,
            Right: _ => false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Renaming_Character_To_Available_Name_Should_Succeed()
    {
        var result = await _characterService.RenameCharacter("Chad_Games", "Not_Chad_Games", _token);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Renaming_Character_To_Taken_Name_Return_Name_Is_Taken()
    {
        var result = await _characterService.RenameCharacter("Chad_Games", "Mr_Trent", _token);

        result.Match(
            err => err == FLAdminError.CharacterNameIsTaken,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Renaming_Nonexistent_Character_Should_Return_Character_Not_Found()
    {
        var result = await _characterService.RenameCharacter("Not_Chad_Games", "More_Not_Chad_Games", _token);

        result.Match(
            err => err == FLAdminError.CharacterNotFound,
            false
        ).Should().BeTrue();
    }


    [Fact]
    public async Task When_Moving_Character_Should_Succeed()
    {
        var result = await _characterService.MoveCharacter("Chad_Games", "abc123456", _token);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Moving_Character_To_Nonexistent_Account_Should_Return_Account_Not_Found()
    {
        var result = await _characterService.MoveCharacter("Chad_Games", "123", _token);

        result.Match(
            err => err == FLAdminError.AccountNotFound,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Nonexistent_Character_Should_Return_Character_Not_Found()
    {
        var result = await _characterService.MoveCharacter("Not_Chad_Games", "abc123456", _token);

        result.Match(
            err => err == FLAdminError.CharacterNotFound,
            false
        ).Should().BeTrue();
    }
}