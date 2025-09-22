using System.ComponentModel;

namespace FlAdmin.Common.Models.Error;

public enum FLAdminError
{
    Unknown = 0,
    
    [Description("Request was cancelled")]
    RequestCancelled = 10,
    
    [Description("Hangfire scheduler failed")]
    HangfireFailure = 20,

    [Description("Database encountered an unexpected error.")]
    DatabaseError = 1000,

    [Description("A query or closure was attempted when a Mongo session was not started.")]
    SessionNotStarted,

    [Description("An exception occured when attempting to run a command to the database.")]
    CommandError,

    [Description("A session is already started, finish or abort the currrent session before starting a new one.")]
    SessionAlreadyExists,

    [Description("Session ID mismatch.")] SessionIdMismatch,

    [Description("No accounts were found that match the provided criteria.")]
    AccountNotFound = 2000,

    [Description("Provided id is null, all accounts require an account Id.")]
    AccountIdIsNull,

    [Description("An account with that username already exists.")]
    UsernameAlreadyExists,

    [Description("Provided password and username do not match.")]
    IncorrectPassword,

    [Description("The type provided does not match the type on the document in the database.")]
    AccountElementTypeMismatch,

    [Description("This account already has a username.")]
    AccountAlreadyHasUsername,

    [Description("The specified field does not exist on this document.")]
    AccountFieldDoesNotExist,

    [Description("An account with this id already exists")]
    AccountIdAlreadyExists,

    [Description("Editing this account or its fields is disallowed.")]
    AccountIsProtected,

    [Description("Editing this field is disallowed.")]
    AccountFieldIsProtected,

    [Description("SuperAdmin may not be granted as a role to any account nor removed.")]
    SuperAdminRoleIsProtected,

    [Description("A field with this name already exists.")]
    AccountFieldAlreadyExists,

    [Description("That operation would result in too many characters existing on that account.")]
    AccountTooManyCharacters,

    [Description("Character with that name or ID already exists.")]
    CharacterAlreadyExists = 3000,

    [Description("That name is already taken by another character.")]
    CharacterNameIsTaken,

    [Description("Character has invalid parameters.")]
    InvalidCharacter,

    [Description("CharacterID is Null.")] CharacterIdIsNull,

    [Description("Character does not exist on the database.")]
    CharacterNotFound,

    [Description("A field with that name already exists.")]
    CharacterFieldAlreadyExists,

    [Description("That field is protected and cannot be changed.")]
    CharacterFieldIsProtected,

    [Description("That field does not exist.")]
    CharacterFieldDoesNotExist,

    [Description("Type provided does not match the type of the field on the database.")]
    CharacterElementTypeMismatch,

    [Description("The character provided has invalid fields.")]
    CharacterInvalidCharacter,

    [Description("An error occured when attempting to modify the account associated with this character.")]
    CharacterAccountError,

    [Description(
        "The character is either logged in or can't be confirmed that they are logged off and thus the operation can't be performed.")]
    CharacterIsLoggedIn,

    [Description("FLAdmin was unable to initialize FLServer.")]
    FLServerFailedToStart = 4000,

    [Description("FLHook failed to initialize alongside FLServer.")]
    FLHookFailedToStart,

    [Description("FLAdmin was unable to terminate FLServer.")]
    FlServerFailedToTerminate,

    [Description("HTTP Request to FLHook timed out.")]
    FLHookRequestTimeout,

    [Description("FlHook encountered an error.")]
    FlHookHttpError,

    [Description("FlHook is failing to respond and likely frozen.")]
    FlHookFailedToRespond,
    
    [Description("Flserver is already online")]
    ServerAlreadyOnline,

    [Description("File not found at that specified path")]
    FileNotFound = 5000,

    [Description("The provided file is not a valid JSON file.")]
    FileNotValidJson
}