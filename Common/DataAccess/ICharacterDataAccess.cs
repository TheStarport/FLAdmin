using System.Linq.Expressions;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.DataAccess;

public interface ICharacterDataAccess
{
    Task<Option<FLAdminErrorCode>> CreateCharacters(CancellationToken token, params Character[] characters);

    Task<Option<FLAdminErrorCode>> UpdateCharacter(BsonDocument character, CancellationToken token);

    //Note this function does not account for any potential dangling references of accounts. 
    Task<Option<FLAdminErrorCode>> DeleteCharacters(CancellationToken token, params string[] characters);

    Task<Either<FLAdminErrorCode, Character>> GetCharacter(Either<ObjectId, string> characterName, CancellationToken token);

    Task<Option<FLAdminErrorCode>> CreateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName,
        T value, CancellationToken token);

    Task<Option<FLAdminErrorCode>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName,
        T value, CancellationToken token);

    Task<Option<FLAdminErrorCode>> RemoveFieldOnCharacter(Either<ObjectId, string> character, string fieldName,
        CancellationToken token);

    Task<List<Character>> GetCharactersByFilter(Expression<Func<Character, bool>> filter, CancellationToken token,
        int page = 1,
        int pageSize = 100);

    Task<List<Character>> GetCharactersByFilter(BsonDocument filter, CancellationToken token, int page = 1,
        int pageSize = 100);
}