using System.Linq.Expressions;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.DataAccess;

public interface ICharacterDataAccess
{
    Task<Option<FLAdminError>> CreateCharacters(params Character[] characters);
    
    Task<Option<FLAdminError>> UpdateCharacter(BsonDocument character);
    
    //Note this function does not account for any potential dangling references of accounts. 
    Task<Option<FLAdminError>> DeleteCharacters(params string[] characters);
    
    Task<Either<FLAdminError, Character>> GetCharacter(Either<ObjectId, string> characterName);
    
    Task<Option<FLAdminError>> CreateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName,
        T value);
    
    Task<Option<FLAdminError>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName,
        T value);
    
    Task<Option<FLAdminError>> RemoveFieldOnCharacter(Either<ObjectId, string> character, string fieldName);
    
    Task<List<Character>> GetCharactersByFilter(Expression<Func<Character, bool>> filter, int page = 1, int pageSize = 100);
}