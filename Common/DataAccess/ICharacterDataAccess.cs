using System.Linq.Expressions;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.DataAccess;

public interface ICharacterDataAccess
{
    Task<Option<FLAdminError>> CreateCharacters(CancellationToken token, params Character[] characters);

    Task<Option<FLAdminError>> UpdateCharacter(BsonDocument character, CancellationToken token);

    //Note this function does not account for any potential dangling references of accounts. 
    Task<Option<FLAdminError>> DeleteCharacters(CancellationToken token, params string[] characters);

    Task<Either<FLAdminError, Character>> GetCharacter(Either<ObjectId, string> characterName, CancellationToken token);

    Task<Option<FLAdminError>> CreateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName,
        T value, CancellationToken token);

    Task<Option<FLAdminError>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName,
        T value, CancellationToken token);

    Task<Option<FLAdminError>> RemoveFieldOnCharacter(Either<ObjectId, string> character, string fieldName,
        CancellationToken token);

    Task<List<Character>> GetCharactersByFilter(Expression<Func<Character, bool>> filter, CancellationToken token,
        int page = 1,
        int pageSize = 100);

    Task<List<Character>> GetCharactersByFilter(BsonDocument filter, CancellationToken token, int page = 1,
        int pageSize = 100);
}