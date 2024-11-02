using System.ComponentModel;

namespace FlAdmin.Common.Models.Error;

public enum CharacterError
{
    [Description("Database encountered an unexpected error.")]
    DatabaseError,
    [Description("Character with that name or ID already exists.")]
    CharacterAlreadyExists,
    [Description("CharacterID is Null.")]
    CharacterIdIsNull,
    [Description("Character does not exist on the database.")]
    CharacterNotFound,
    [Description("A field with that name already exists.")]
    FieldAlreadyExists,
    [Description("That field is protected and cannot be changed.")]
    FieldIsProtected,
    [Description("That field does not exist.")]
    FieldDoesNotExist,
    [Description("Type provided does not match the type of the field on the database.")]
    ElementTypeMismatch,
    [Description("The character provided has invalid fields.")]
    InvalidCharacter,
    [Description("An error occured when attempting to modify the account associated with this character.")]
    AccountError,
    [Description("That character is currently logged into the game therefor operation cannot be performed.")]
    CharacterIsLoggedIn,
}