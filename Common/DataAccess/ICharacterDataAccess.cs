using System.Linq.Expressions;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.DataAccess;

public interface ICharacterDataAccess
{
    
    /// <summary>
    /// Creates characters on the database.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="characters">Charaacters to be added.</param>
    /// <returns>Error result if it fails.</returns>
    Task<Option<ErrorResult>> CreateCharacters(CancellationToken token, params Character[] characters);

    /// <summary>
    /// Updates a character on the database via a differential update.
    /// </summary>
    /// <param name="character">Update document</param>
    /// <param name="token"></param>
    /// <returns>Error result if it fails.</returns>
    Task<Option<ErrorResult>> UpdateCharacter(BsonDocument character, CancellationToken token);

    
    /// <summary>
    /// Deletes characters on the database.
    /// Note this function does not account for any potential dangling references of accounts. 
    /// </summary>
    /// <param name="token"></param>
    /// <param name="characters">characters to be deleted.</param>
    /// <returns>Error result if it fails.</returns>
    Task<Option<ErrorResult>> DeleteCharacters(CancellationToken token, params string[] characters);

    /// <summary>
    /// Retrieves a specified character from the database.
    /// </summary>
    /// <param name="characterName">Either the internal objectId or the characters name.</param>
    /// <param name="token"></param>
    /// <returns>Character if successful, Error result if failed.</returns>
    Task<Either<ErrorResult, Character>> GetCharacter(Either<ObjectId, string> characterName, CancellationToken token);

    /// <summary>
    /// Adds a provided field with the provided value on a specified character.
    /// </summary>
    /// <param name="character">Either the internal objectId or the characters name.</param>
    /// <param name="fieldName">name of the field to be added.</param>
    /// <param name="value">Value of the field to be added.</param>
    /// <param name="token"></param>
    /// <typeparam name="T">Type of the field</typeparam>
    /// <returns>Error result if failed.</returns>
    Task<Option<ErrorResult>> CreateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName,
        T value, CancellationToken token);

    /// <summary>
    /// Updates an already existing field on a specified character with a new value, types must match to succeed.
    /// </summary>
    /// <param name="character">Either the internal objectId or the characters name.</param>
    /// <param name="fieldName">Field name to be updated.</param>
    /// <param name="value">New value.</param>
    /// <param name="token"></param>
    /// <typeparam name="T">Type of the field.</typeparam>
    /// <returns>Error result if it failed.</returns>
    Task<Option<ErrorResult>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName,
        T value, CancellationToken token);

    /// <summary>
    /// Removes a specified field from the character, ID and name cannot be removed.
    /// </summary>
    /// <param name="character">Either the internal objectId or the characters name.</param>
    /// <param name="fieldName">name of field to be removed.</param>
    /// <param name="token"></param>
    /// <returns>Error result if it failed.</returns>
    Task<Option<ErrorResult>> RemoveFieldOnCharacter(Either<ObjectId, string> character, string fieldName,
        CancellationToken token);

    /// <summary>
    /// Grabs characters from the database based on a C# expression filter.
    /// </summary>
    /// <param name="filter">C# expression filter.</param>
    /// <param name="token"></param>
    /// <param name="page">page number to grab</param>
    /// <param name="pageSize">number of characters per page.</param>
    /// <returns>List of characters, empty if none are found or an error occurs.</returns>
    Task<List<Character>> GetCharactersByFilter(Expression<Func<Character, bool>> filter, CancellationToken token,
        int page = 1,
        int pageSize = 100);

    /// <summary>
    /// Grabs characters from the database based on a Bson Document filter.
    /// </summary>
    /// <param name="filter">BSon document filter, see mongoDB documentation.</param>
    /// <param name="token"></param>
    /// <param name="page">page number to grab.</param>
    /// <param name="pageSize">number of characters per page.</param>
    /// <returns>List of characters, empty if none are found or an error occurs.</returns>
    Task<List<Character>> GetCharactersByFilter(BsonDocument filter, CancellationToken token, int page = 1,
        int pageSize = 100);
}