using System.Linq.Expressions;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;

namespace FlAdmin.Common.DataAccess;

public interface ICharacterDataAccess
{
    Task<Option<CharacterError>> CreateCharacters(params Character[] characters);
    
    Task<Option<CharacterError>> UpdateCharacter(Character character);
    
    Task<Option<CharacterError>> DeleteCharacters(params Character[] characters);
    
    Task<Either<CharacterError, Character>> GetCharacterByName(string characterName);
    
    Task<Option<CharacterError>> CreateFieldOnCharacter<T>(Character character, string fieldName ,T value);
    
    Task<Option<CharacterError>> UpdateFieldOnCharacter<T>(Character character, string fieldName , T value);
    
    Task<Option<CharacterError>> RemoveFieldOnCharacter(Character character, string fieldName);
    
    List<Character> GetCharactersByFilter(Expression<Func<Character, bool>> filter, int page = 1, int pageSize = 100);
}