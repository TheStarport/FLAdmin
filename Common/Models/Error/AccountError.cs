using System.ComponentModel;

namespace FlAdmin.Common.Models.Error;

public enum AccountError
{
    [Description("No accounts were found that match the provided criteria.")]
    AccountNotFound,
    [Description("Provided id is null, all accounts require an account Id.")]
    AccountIdIsNull,
    [Description("An account with that username already exists.")]  
    UsernameAlreadyExists,
    [Description("Provided password and username do not match.")]
    IncorrectPassword,
    [Description("Database encountered an unexpected error.")]
    DatabaseError,
    [Description("The type provided does not match the type on the document in the database.")]
    ElementTypeMismatch,
    [Description("The type provided does not match the type on the document in the database.")]
    AccountAlreadyHasUsername,
    [Description("The specified field does not exist on this document.")]
    FieldDoesNotExist,
}