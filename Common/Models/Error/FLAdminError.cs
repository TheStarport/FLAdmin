using System.ComponentModel;

namespace FlAdmin.Common.Models.Error;

public enum FLAdminError
{
    Unknown = 0,

    [Description("Database encountered an unexpected error.")]
    DatabaseError = 1000,

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

    [Description("A field with this name already exists.")]
    AccountFieldAlreadyExists,

    [Description("Character with that name or ID already exists.")]
    CharacterAlreadyExists = 3000,
    
    [Description("CharacterID is Null.")] 
    InvalidCharacter,
    
    [Description("CharacterID is Null.")] 
    CharacterIdIsNull,

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

    [Description("That character is currently logged into the game therefor operation cannot be performed.")]
    CharacterIsLoggedIn,
}