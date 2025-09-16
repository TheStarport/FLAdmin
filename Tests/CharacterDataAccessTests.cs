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
    private readonly ICharacterDataAccess _characterDataAccess;
    private readonly EphemeralTestDatabase _fixture;

    public CharacterDataAccessTests()
    {
        _fixture = new EphemeralTestDatabase();
        _characterDataAccess = new CharacterDataAccess(_fixture.DatabaseAccess, _fixture.Config,
            new NullLogger<CharacterDataAccess>());
    }

    private CancellationToken token = CancellationToken.None;

    public void Dispose()
    {
        _fixture.Dispose();
    }

    [Fact]
    public async Task When_Grabbing_Characters_Then_List_Should_Be_Non_Empty()
    {
        var charList = await _characterDataAccess.GetCharactersByFilter(x => true, token);

        charList.Count.Should().Be(100);
    }

    [Fact]
    public async Task When_Grabbing_Specific_Character_Should_Be_Successfully_Grabbed()
    {
        var character = await _characterDataAccess.GetCharacter("Chad_Games", token);

        character.IsRight.Should().BeTrue();
    }

    [Fact]
    public async Task When_Grabbing_Character_By_ObjectId_Should_Be_Successfully_Grabbed()
    {
        var character = await _characterDataAccess.GetCharacter(new ObjectId("65d3abc10f019879e20193d2"), token);

        character.IsRight.Should().BeTrue();
    }


    [Fact]
    public async Task When_Grabbing_Non_Existent_Character_Should_Return_Character_Not_Found()
    {
        var character = await _characterDataAccess.GetCharacter("Not_Chad_Games", token);

        character.Match(
            _ => false,
            err => err == FLAdminError.CharacterNotFound
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Updating_Valid_Character_Should_Not_Return_Error()
    {
        var testCharacter = new Character
        {
            CharacterName = "Chad_Games",
            Id = new ObjectId("65d3abc10f019879e20193d2"),
            Money = 123456
        };

        var result = await _characterDataAccess.UpdateCharacter(testCharacter.ToBsonDocument(), token);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Updating_Character_Name_To_Pre_Existing_Name_Should_Return_Character_Already_Exists()
    {
        var testCharacter = new Character
        {
            CharacterName = "Mr_Trent",
            Id = new ObjectId("65d3abc10f019879e20193d2"),
            Money = 12345787,
            AccountId = "123abc456"
        };

        var result = await _characterDataAccess.UpdateCharacter(testCharacter.ToBsonDocument(), token);

        result.Match(
            x => x == FLAdminError.CharacterNameIsTaken,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Updating_Non_Existent_Character_Should_Return_Character_Not_Found()
    {
        var testCharacter = new Character
        {
            CharacterName = "Not_Chad_Games",
            Id = new ObjectId("65d3abc10f019879e20193d3"),
            Money = 123456
        };

        var result = await _characterDataAccess.UpdateCharacter(testCharacter.ToBsonDocument(), token);

        result.Match(
            x => x == FLAdminError.CharacterNotFound,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Update_With_Malformed_Bson_Document_Should_Character_Id_Is_Null()
    {
        var testCharacter = new Character
        {
            CharacterName = "Not_Chad_Games",
            Id = new ObjectId("65d3abc10f019879e20193d2"),
            Money = 123456
        };

        var testDoc = testCharacter.ToBsonDocument();
        testDoc.Remove("_id");

        var result = await _characterDataAccess.UpdateCharacter(testDoc, token);

        result.Match(
            x => x == FLAdminError.CharacterIdIsNull,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Deleting_Existing_Character_Should_Not_Return_Error()
    {
        var result = await _characterDataAccess.DeleteCharacters(token, "Chad_Games");

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Deleting_Non_Existing_Character_Should_Return_Character_Not_Found()
    {
        var result = await _characterDataAccess.DeleteCharacters(token, "Not_Chad_Games");

        result.Match(
            x => x == FLAdminError.CharacterNotFound,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Deleting_Multiple_Characters_And_One_Does_Not_Exist_Should_Return_Character_Not_Found()
    {
        var result = await _characterDataAccess.DeleteCharacters(token, "Chad_Games", "Not_Chad_Games");

        result.Match(
            x => x == FLAdminError.CharacterNotFound,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Creating_Character_Should_Not_Error()
    {
        var testCharacter = new Character
        {
            CharacterName = "Chad_Games_Jr",
            Id = ObjectId.GenerateNewId(),
            Money = 12345678
        };

        var result = await _characterDataAccess.CreateCharacters(token, testCharacter);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Creating_Character_With_Same_Name_Should_Return_Character_Already_Exists()
    {
        var testCharacter = new Character
        {
            CharacterName = "Chad_Games",
            Id = ObjectId.GenerateNewId(),
            Money = 12345678
        };

        var result = await _characterDataAccess.CreateCharacters(token, testCharacter);

        result.Match(
            x => x == FLAdminError.CharacterAlreadyExists,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Creating_Field_On_Character_Should_Succeed()
    {
        var result = await _characterDataAccess.CreateFieldOnCharacter("Chad_Games", "someFactor", 1234567, token);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Creating_Field_That_Already_Exists_Should_Return_Field_Already_Exists()
    {
        var result = await _characterDataAccess.CreateFieldOnCharacter("Chad_Games", "money", 1234567, token);

        result.Match(
            x => x == FLAdminError.CharacterFieldAlreadyExists,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Creating_Field_On_Non_Existent_Character_Should_Return_Character_Not_Found()
    {
        var result = await _characterDataAccess.CreateFieldOnCharacter("Not_Chad_Games", "someFactor", 1234567, token);

        result.Match(
            x => x == FLAdminError.CharacterNotFound,
            false
        ).Should().BeTrue();
    }


    [Fact]
    public async Task When_Deleting_Existing_Field_Should_Succeed()
    {
        var result = await _characterDataAccess.RemoveFieldOnCharacter("Chad_Games", "money", token);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Deleting_Non_Existing_Field_Should_Return_Field_Does_Not_Exist()
    {
        var result = await _characterDataAccess.RemoveFieldOnCharacter("Chad_Games", "garg", token);

        result.Match(
            x => x == FLAdminError.CharacterFieldDoesNotExist,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Deleting_Name_Should_Return_Character_Field_Is_Protected()
    {
        var result = await _characterDataAccess.RemoveFieldOnCharacter("Chad_Games", "characterName", token);

        result.Match(
            x => x == FLAdminError.CharacterFieldIsProtected,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task
        When_Attempting_To_Delete_Field_From_Non_Existing_Character_Should_Return_Character_Not_Found()
    {
        var result = await _characterDataAccess.RemoveFieldOnCharacter("Not_Chad_Games", "money", token);

        result.Match(
            x => x == FLAdminError.CharacterNotFound,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Updating_Field_Should_Succeed()
    {
        var result = await _characterDataAccess.UpdateFieldOnCharacter("Chad_Games", "money", 1234567, token);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public async Task When_Updating_Field_With_Wrong_Type_Should_Return_Element_Type_Mismatch()
    {
        var result = await _characterDataAccess.UpdateFieldOnCharacter("Chad_Games", "money", "1234567", token);

        result.Match(
            x => x == FLAdminError.CharacterElementTypeMismatch,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Update_Id_Should_Return_Field_Is_Protected()
    {
        var result =
            await _characterDataAccess.UpdateFieldOnCharacter("Chad_Games", "_id", ObjectId.GenerateNewId(), token);

        result.Match(
            x => x == FLAdminError.CharacterFieldIsProtected,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Update_Name_To_A_Taken_Name_Should_Return_Name_Is_Taken()
    {
        var result =
            await _characterDataAccess.UpdateFieldOnCharacter("Chad_Games", "characterName", "Mr_Trent", token);

        result.Match(
            x => x == FLAdminError.CharacterNameIsTaken,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Update_Non_Existing_Field_Should_Return_Field_Does_Not_Exist()
    {
        var result = await _characterDataAccess.UpdateFieldOnCharacter("Chad_Games", "gagdr", 1234567, token);

        result.Match(
            x => x == FLAdminError.CharacterFieldDoesNotExist,
            false
        ).Should().BeTrue();
    }

    [Fact]
    public async Task When_Attempting_To_Update_Field_On_Nonexistent_Character_Should_Return_Character_Not_Found()
    {
        var result = await _characterDataAccess.UpdateFieldOnCharacter("Not_Chad_Games", "money", 1234567, token);

        result.Match(
            x => x == FLAdminError.CharacterNotFound,
            false
        ).Should().BeTrue();
    }
}