using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface ICharacterService
{
    Task<Either<CharacterError, List<Character>>> GetCharactersOfAccount(string accountId);
    Task<Either<CharacterError, Character>> GetCharacterByName(string name);
    Task<List<Character>> QueryCharacters(IQueryable<Character> query);
    Task<Option<CharacterError>> AddCharacter(Character character);

    Task<Option<CharacterError>> DeleteAllCharactersOnAccount(string accountId);
    Task<Option<CharacterError>> DeleteCharacter(Either<ObjectId, string> character);
    Task<Option<CharacterError>> MoveCharacter(Either<ObjectId, string> character, string newAccountId);

    Task<Option<CharacterError>> UpdateCharacter(Character character);
    Task<Option<CharacterError>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character,string fieldName, T value);

    Task<Option<CharacterError>> RemoveFieldOnCharacter(Either<ObjectId, string> character, string fieldName);
    Task<Option<CharacterError>> AddFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName, T value);
}