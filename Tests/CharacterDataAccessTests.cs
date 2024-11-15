using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Logic.DataAccess;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;

namespace FlAdmin.Tests;

[Collection("DatabaseTests")]
public class CharacterDataAccessTests : IDisposable
{
    private readonly EphemeralTestDatabase _fixture;
    private readonly ICharacterDataAccess _characterDataAccess;

    public CharacterDataAccessTests()
    {
        _fixture = new EphemeralTestDatabase();
        _characterDataAccess = new CharacterDataAccess(_fixture.DatabaseAccess, _fixture.Config,
            new NullLogger<CharacterDataAccess>());
    }

    [Fact]
    public async Task When_Grabbing_Characters_Then_List_Should_Be_Non_Empty()
    {
        var charList = await _characterDataAccess.GetCharactersByFilter(x => true, 1, 100);

        charList.Count.Should().Be(100);
    }

    [Fact]
    public async Task When_Grabbing_Specific_Character_Should_Be_Successfully_Grabbed()
    {
        var character = await _characterDataAccess.GetCharacter("Chad_Games");

        character.IsRight.Should().BeTrue();
    }

    [Fact]
    public async Task When_Grabbing_Non_Existent_Character_Should_Return_Character_Not_Found()
    {
        var character = await _characterDataAccess.GetCharacter("Sir_Not_Appearing_In_This_Game");

        character.Match(
            x => false,
            err => err == FLAdminError.CharacterNotFound
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Updating_Valid_Character_Should_Not_Return_Error()
    {
        var testCharacter = new Character()
        {
            CharacterName = "Chad_Games",
            Id = new ObjectId("65d3abc10f019879e20193d1"),
            Money = 123456
        };

        var result = await _characterDataAccess.UpdateCharacter(testCharacter.ToBsonDocument());

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Updating_Non_Existent_Character_Should_Return_Character_Not_Found()
    {
        var testCharacter = new Character()
        {
            CharacterName = "Not_Chad_Games",
            Id = new ObjectId("65d3abc10f019879e20193d2"),
            Money = 123456
        };

        var result = await _characterDataAccess.UpdateCharacter(testCharacter.ToBsonDocument());

        result.Match(
            Some: x => x == FLAdminError.CharacterNotFound,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Update_With_Malformed_Bson_Document_Should_Character_Id_Is_Null()
    {
        var testCharacter = new Character()
        {
            CharacterName = "Not_Chad_Games",
            Id = new ObjectId("65d3abc10f019879e20193d2"),
            Money = 123456
        };

        var testDoc = testCharacter.ToBsonDocument();
        testDoc.Remove("_id");

        var result = await _characterDataAccess.UpdateCharacter(testDoc);

        result.Match(
            Some: x => x == FLAdminError.CharacterIdIsNull,
            false
        ).Should().BeTrue();
    }


    public void Dispose()
    {
        _fixture.Dispose();
    }
}