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
}