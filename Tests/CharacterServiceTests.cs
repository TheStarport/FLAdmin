using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services.Database;
using FlAdmin.Tests.DataAccessMocks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlAdmin.Tests;

public class CharacterServiceTests
{
    private CharacterService _characterService;

    public CharacterServiceTests()
    {
        ICharacterDataAccess characterDataAccess = new CharacterDataAccessMock();
        IAccountDataAccess accountDataAccess = new AccountDataAccessMock();
        IValidationService validationService = new ValidationServiceMock();


        _characterService = new CharacterService(characterDataAccess, accountDataAccess, validationService,
            new FlAdminConfig(), new NullLogger<CharacterService>());
    }

    //Simple test to see mocks are working correctly. 
    [Fact]
    public async Task Should_Get_Character()
    {
        var result = await _characterService.GetCharacterByName("Chad_Games");

        result.IsRight.Should().BeTrue();
    }

    [Fact]
    public async Task When_Removing_Characters_From_Account_Should_Not_Error()
    {
        var result = await _characterService.DeleteAllCharactersOnAccount("123abc456");

        result.IsNone.Should().BeTrue();
    }


    [Fact]
    public async Task When_Getting_Characters_From_Account_Should_Not_Error()
    {
        var result = await _characterService.GetCharactersOfAccount("123abc456");

        result.Match(
            Left: _ => false,
            Right: x => x.Count == 2
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Getting_Characters_From_Nonexistent_Account_Should_Return_Account_Not_Found()
    {
        var result = await _characterService.GetCharactersOfAccount("123");
        
        result.Match(
            Left: err => err == FLAdminError.AccountNotFound,
            Right: _ => false
            ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Getting_Characters_From_Account_Without_Characters_Should_Return_Character_Not_Found()
    {
        var result = await _characterService.GetCharactersOfAccount("abc123456");
        
        result.Match(
            Left: err => err == FLAdminError.CharacterNotFound,
            Right: _ => false
        ).Should().BeTrue();
    }
    
    
}