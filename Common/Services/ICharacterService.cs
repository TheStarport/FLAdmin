using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface ICharacterService
{
    Task<Either<FLAdminError, List<Character>>> GetCharactersOfAccount(string accountId);
    Task<Either<FLAdminError, Character>> GetCharacterByName(string name);
    Task<List<Character>> QueryCharacters(IQueryable<Character> query);
    Task<Option<FLAdminError>> AddCharacter(Character character);

    Task<Option<FLAdminError>> DeleteAllCharactersOnAccount(string accountId);
    Task<Option<FLAdminError>> DeleteCharacter(Either<ObjectId, string> character);
    Task<Option<FLAdminError>> MoveCharacter(Either<ObjectId, string> character, string newAccountId);

    Task<Option<FLAdminError>> UpdateCharacter(Character character);
    Task<Option<FLAdminError>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName, T value);

    Task<Option<FLAdminError>> RemoveFieldOnCharacter(Either<ObjectId, string> character, string fieldName);
    Task<Option<FLAdminError>> AddFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName, T value);

    Task<Option<FLAdminError>> RenameCharacter(string oldName, string newName);
}