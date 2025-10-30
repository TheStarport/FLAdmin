using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface ICharacterService
{
    /// <summary>
    ///     Grabs a list of characters that exist on the specified account.
    /// </summary>
    /// <param name="accountId"> ID of the account to grab all the characters from.</param>
    /// <param name="token"></param>
    /// <returns>
    ///     A "Left/Right" Either object, Left being an error if the operation was a failure, Right being a List of
    ///     Characters if the operation was successful.
    /// </returns>
    Task<Either<FLAdminErrorCode, List<Character>>> GetCharactersOfAccount(string accountId, CancellationToken token);

    /// <summary>
    ///     Gets the character specified by the name.
    /// </summary>
    /// <param name="name"> Name of the character to be grabbed</param>
    /// <param name="token"></param>
    /// <returns>
    ///     A "Left/Right" Either object, Left being an error if the operation was a failure, Right being the Desired
    ///     character if the operation was successful.
    /// </returns>
    Task<Either<FLAdminErrorCode, Character>> GetCharacterByName(string name, CancellationToken token);

    /// <summary>
    ///     Adds a character to the database
    /// </summary>
    /// <param name="character"> Character to be added to the database.</param>
    /// <param name="token"></param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminErrorCode>> AddCharacter(Character character, CancellationToken token);

    /// <summary>
    ///     Deletes all characters on the specified account
    /// </summary>
    /// <param name="accountId"> Account to delete all the characters of.</param>
    /// <param name="token"></param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminErrorCode>> DeleteAllCharactersOnAccount(string accountId, CancellationToken token);

    /// <summary>
    ///     Deletes the specified character
    /// </summary>
    /// <param name="character"> Character to be deleted, either by OID or character name </param>
    /// <param name="token"></param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminErrorCode>> DeleteCharacter(Either<ObjectId, string> character, CancellationToken token);


    /// <summary>
    ///     Moves the character onto the specified account.
    /// </summary>
    /// <param name="character"> The character to be moved, either as an OID or the character's name</param>
    /// <param name="newAccountId"> The account the character is to be moved to</param>
    /// <param name="token"></param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminErrorCode>> MoveCharacter(Either<ObjectId, string> character, string newAccountId,
        CancellationToken token);

    /// <summary>
    ///     Replaces the character on the database with the provided character struct of the same matching OID,
    ///     The provided document must have a valid OID or will return an error.
    /// </summary>
    /// <param name="character"> The character struct that will replace the document on the database</param>
    /// <param name="token"></param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminErrorCode>> UpdateCharacter(Character character, CancellationToken token);


    /// <summary>
    ///     Updates a specified field on a specified character.
    /// </summary>
    /// <param name="character">The character, either as an OID or the character's name</param>
    /// <param name="fieldName">Name of the field to be updated.</param>
    /// <param name="value"> The value of the field to be updated </param>
    /// <param name="token"></param>
    /// <typeparam name="T"> The Type of the param, often inferred </typeparam>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminErrorCode>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName, T value,
        CancellationToken token);

    /// <summary>
    ///     Removes a specified field on a specified character. Note: Only EXTRA fields are
    ///     deletable, default fields that are defined in the character struct are not allowed to be deleted through the api
    /// </summary>
    /// <param name="character">The character, either as an OID or the character's name.</param>
    /// <param name="fieldName">Name of the field to be removed.</param>
    /// <param name="token"></param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminErrorCode>> RemoveFieldOnCharacter(Either<ObjectId, string> character, string fieldName,
        CancellationToken token);

    /// <summary>
    ///     Adds a specified field on a specified character.
    /// </summary>
    /// <param name="character">The character, either as an OID or the character's name.</param>
    /// <param name="fieldName">Name of the field to be added.</param>
    /// <param name="value">Value of the field to be added</param>
    /// <param name="token"></param>
    /// <typeparam name="T">Type of the field, must be a BSON supported type.</typeparam>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminErrorCode>> AddFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName, T value,
        CancellationToken token);

    /// <summary>
    ///     Renames a character.
    /// </summary>
    /// <param name="oldName">Name of the character to be renamed</param>
    /// <param name="newName">New name of the character, must be unique on the database.</param>
    /// <param name="token"></param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminErrorCode>> RenameCharacter(string oldName, string newName, CancellationToken token);

    /// <summary>
    /// Gets a list of truncated character data based on a BSON document filter
    /// </summary>
    /// <param name="filter"> Filter as a bson document, see mongo documentation.</param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<Either<FLAdminErrorCode, List<CharacterSummary>>> GetCharacterSummaries(BsonDocument filter, int page,
        int pageSize, CancellationToken token);
}