using System.ComponentModel;
using FlAdmin.Common.Models.Error;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Extensions;

public static class ResultExtensions
{
    public static string GetEnumDescription(this Enum value)
    {
        return value.GetType()
            .GetField(value.ToString())
            ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .SingleOrDefault() is not DescriptionAttribute attribute
            ? value.ToString()
            : attribute.Description;
    }

    public static IActionResult ParseAccountError(this AccountError accountError, ControllerBase controller)
    {
        return accountError switch
        {
            AccountError.AccountNotFound => controller.NotFound(accountError.GetEnumDescription()),
            AccountError.AccountIdIsNull => controller.BadRequest(accountError.GetEnumDescription()),
            AccountError.UsernameAlreadyExists => controller.Conflict(accountError.GetEnumDescription()),
            AccountError.IncorrectPassword => controller.Unauthorized(accountError.GetEnumDescription()),
            AccountError.DatabaseError => new ObjectResult(StatusCodes.Status500InternalServerError),
            AccountError.ElementTypeMismatch => controller.BadRequest(accountError.GetEnumDescription()),
            AccountError.AccountAlreadyHasUsername => controller.Conflict(accountError.GetEnumDescription()),
            AccountError.FieldDoesNotExist => controller.NotFound(accountError.GetEnumDescription()),
            AccountError.AccountIdAlreadyExists => controller.Conflict(accountError.GetEnumDescription()),
            AccountError.AccountIsProtected => controller.Forbid(accountError.GetEnumDescription()),
            AccountError.FieldIsProtected => controller.Forbid(accountError.GetEnumDescription()),
            AccountError.FieldAlreadyExists => controller.Conflict(accountError.GetEnumDescription()),
            _ => throw new ArgumentOutOfRangeException(nameof(accountError), accountError, null)
        };
    }

    public static IActionResult ParseAccountError(this CharacterError characterError, ControllerBase controller)
    {
        return characterError switch
        {
            CharacterError.DatabaseError =>  new ObjectResult(StatusCodes.Status500InternalServerError),
            CharacterError.CharacterAlreadyExists => controller.Conflict(characterError.GetEnumDescription()),
            CharacterError.CharacterIdIsNull => controller.BadRequest(characterError.GetEnumDescription()),
            CharacterError.CharacterNotFound => controller.NotFound(characterError.GetEnumDescription()),
            CharacterError.FieldAlreadyExists => controller.Conflict(characterError.GetEnumDescription()),
            CharacterError.FieldIsProtected => controller.Forbid(characterError.GetEnumDescription()),
            CharacterError.FieldDoesNotExist => controller.NotFound(characterError.GetEnumDescription()),
            CharacterError.ElementTypeMismatch => controller.BadRequest(characterError.GetEnumDescription()),
            CharacterError.InvalidCharacter => controller.BadRequest(characterError.GetEnumDescription()),
            CharacterError.AccountError => controller.BadRequest(characterError.GetEnumDescription()),
            CharacterError.CharacterIsLoggedIn => controller.BadRequest(characterError.GetEnumDescription()),
            _ => throw new ArgumentOutOfRangeException(nameof(characterError), characterError, null)
        };
    }
    
}