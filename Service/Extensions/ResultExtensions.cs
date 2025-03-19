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

    public static IActionResult ParseError(this FLAdminError error, ControllerBase controller)
    {
        return error switch
        {
            FLAdminError.DatabaseError => new ObjectResult(StatusCodes.Status500InternalServerError),
            FLAdminError.Unknown => new ObjectResult(StatusCodes.Status500InternalServerError),
            FLAdminError.AccountNotFound => controller.NotFound(error.GetEnumDescription()),
            FLAdminError.AccountIdIsNull => controller.BadRequest(error.GetEnumDescription()),
            FLAdminError.UsernameAlreadyExists => controller.Conflict(error.GetEnumDescription()),
            FLAdminError.IncorrectPassword => controller.Unauthorized(error.GetEnumDescription()),
            FLAdminError.AccountElementTypeMismatch => controller.BadRequest(error.GetEnumDescription()),
            FLAdminError.AccountAlreadyHasUsername => controller.Conflict(error.GetEnumDescription()),
            FLAdminError.AccountFieldDoesNotExist => controller.NotFound(error.GetEnumDescription()),
            FLAdminError.AccountIdAlreadyExists => controller.Conflict(error.GetEnumDescription()),
            FLAdminError.AccountIsProtected => controller.Forbid(error.GetEnumDescription()),
            FLAdminError.AccountFieldIsProtected => controller.Forbid(error.GetEnumDescription()),
            FLAdminError.AccountFieldAlreadyExists => controller.Conflict(error.GetEnumDescription()),
            FLAdminError.CharacterAlreadyExists => controller.Conflict(error.GetEnumDescription()),
            FLAdminError.CharacterIdIsNull => controller.BadRequest(error.GetEnumDescription()),
            FLAdminError.CharacterNotFound => controller.NotFound(error.GetEnumDescription()),
            FLAdminError.CharacterFieldAlreadyExists => controller.Conflict(error.GetEnumDescription()),
            FLAdminError.CharacterFieldIsProtected => controller.Forbid(error.GetEnumDescription()),
            FLAdminError.CharacterFieldDoesNotExist => controller.NotFound(error.GetEnumDescription()),
            FLAdminError.CharacterElementTypeMismatch => controller.BadRequest(error.GetEnumDescription()),
            FLAdminError.CharacterInvalidCharacter => controller.BadRequest(error.GetEnumDescription()),
            FLAdminError.CharacterAccountError => controller.BadRequest(error.GetEnumDescription()),
            FLAdminError.CharacterIsLoggedIn => controller.BadRequest(error.GetEnumDescription()),

            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
        };
    }
}