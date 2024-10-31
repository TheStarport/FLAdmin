using System.Linq.Expressions;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.DataAccess;

public interface ICharacterDataAccess
{
    Task<Option<CharacterError>> CreateCharacters(params Character[] characters);
    
    Task<Option<CharacterError>> UpdateCharacter(BsonDocument character);
    
    Task<Option<CharacterError>> DeleteCharacters(params string[] characters);
    
    Task<Either<CharacterError, Character>> GetCharacterByName(string characterName);
    
    Task<Option<CharacterError>> CreateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName,
        T value);
    
    Task<Option<CharacterError>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName,
        T value);
    
    Task<Option<CharacterError>> RemoveFieldOnCharacter(Either<ObjectId, string> character, string fieldName);
    
    Task<List<Character>> GetCharactersByFilter(Expression<Func<Character, bool>> filter, int page = 1, int pageSize = 100);
}