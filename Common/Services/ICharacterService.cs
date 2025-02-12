using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface ICharacterService
{
    /// <summary>
    /// Grabs a list of characters that exist on the specified account.
    /// </summary>
    /// <param name="accountId"> ID of the account to grab all the characters from.</param>
    /// <returns>A "Left/Right" Either object, Left being an error if the operation was a failure, Right being a List of Characters if the operation was successful.</returns>
    Task<Either<FLAdminError, List<Character>>> GetCharactersOfAccount(string accountId);

    /// <summary>
    /// Gets the character specified by the name.
    /// </summary>
    /// <param name="name"> Name of the character to be grabbed</param>
    /// <returns>A "Left/Right" Either object, Left being an error if the operation was a failure, Right being the Desired character if the operation was successful.</returns>
    Task<Either<FLAdminError, Character>> GetCharacterByName(string name);

    Task<List<Character>> QueryCharacters(IQueryable<Character> query);

    /// <summary>
    /// Adds a character to the database
    /// </summary>
    /// <param name="character"> Character to be added to the database.</param>
    /// <returns>Optional error enum, None means the operation was successful, Some means an error was encountered and the operation failed.</returns>
    Task<Option<FLAdminError>> AddCharacter(Character character);

    /// <summary>
    /// Deletes all characters on the specified account
    /// </summary>
    /// <param name="accountId"> Account to delete all the characters of.</param>
    /// <returns>Optional error enum, None means the operation was successful, Some means an error was encountered and the operation failed.</returns>
    Task<Option<FLAdminError>> DeleteAllCharactersOnAccount(string accountId);

    /// <summary>
    /// Deletes the specified character
    /// </summary>
    /// <param name="character"> Character to be deleted, either by OID or character name </param>
    /// <returns>Optional error enum, None means the operation was successful, Some means an error was encountered and the operation failed.</returns>
    Task<Option<FLAdminError>> DeleteCharacter(Either<ObjectId, string> character);


    /// <summary>
    /// Moves the character onto the specified account.
    /// </summary>
    /// <param name="character"> The character to be moved, either as an OID or the character's name</param>
    /// <param name="newAccountId"> The account the character is to be moved to</param>
    /// <returns>Optional error enum, None means the operation was successful, Some means an error was encountered and the operation failed.</returns>
    Task<Option<FLAdminError>> MoveCharacter(Either<ObjectId, string> character, string newAccountId);

    /// <summary>
    /// Replaces the character on the database with the provided character struct of the same matching OID,
    /// The provided document must have a valid OID or will return an error. 
    /// </summary>
    /// <param name="character"> The character struct that will replace the document on the database</param>
    /// <returns>Optional error enum, None means the operation was successful, Some means an error was encountered and the operation failed.</returns>
    Task<Option<FLAdminError>> UpdateCharacter(Character character);


    /// <summary>
    /// Updates a specified field on a specified character. 
    /// </summary>
    /// <param name="character">The character, either as an OID or the character's name</param>
    /// <param name="fieldName">Name of the field to be updated.</param>
    /// <param name="value"> The value of the field to be updated </param>
    /// <typeparam name="T"> The Type of the param, often inferred </typeparam>
    /// <returns>Optional error enum, None means the operation was successful, Some means an error was encountered and the operation failed.</returns>
    Task<Option<FLAdminError>> UpdateFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName, T value);

    /// <summary>
    ///  Removes a specified field on a specified character. Note: Only EXTRA fields are
    /// deletable, default fields that are defined in the character struct are not allowed to be deleted through the api
    /// </summary>
    /// <param name="character">The character, either as an OID or the character's name.</param>
    /// <param name="fieldName">Name of the field to be removed.</param>
    /// <returns>Optional error enum, None means the operation was successful, Some means an error was encountered and the operation failed.</returns>
    Task<Option<FLAdminError>> RemoveFieldOnCharacter(Either<ObjectId, string> character, string fieldName);

    /// <summary>
    /// Adds a specified field on a specified character.
    /// </summary>
    /// <param name="character">The character, either as an OID or the character's name.</param>
    /// <param name="fieldName">Name of the field to be added.</param>
    /// <param name="value">Value of the field to be added</param>
    /// <typeparam name="T">Type of the field, must be a BSON supported type.</typeparam>
    /// <returns>Optional error enum, None means the operation was successful, Some means an error was encountered and the operation failed.</returns>
    Task<Option<FLAdminError>> AddFieldOnCharacter<T>(Either<ObjectId, string> character, string fieldName, T value);

    /// <summary>
    /// Renames a character.
    /// </summary>
    /// <param name="oldName">Name of the character to be renamed</param>
    /// <param name="newName">New name of the character, must be unique on the database.</param>
    /// <returns>Optional error enum, None means the operation was successful, Some means an error was encountered and the operation failed.</returns>
    Task<Option<FLAdminError>> RenameCharacter(string oldName, string newName);
}