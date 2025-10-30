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

    public static IActionResult ParseError(this FLAdminErrorCode errorCode, ControllerBase controller)
    {
        return errorCode switch
        {
            FLAdminErrorCode.DatabaseError => new ObjectResult(StatusCodes.Status500InternalServerError),
            FLAdminErrorCode.Unknown => new ObjectResult(StatusCodes.Status500InternalServerError),
            FLAdminErrorCode.AccountNotFound => controller.NotFound(errorCode.GetEnumDescription()),
            FLAdminErrorCode.AccountIdIsNull => controller.BadRequest(errorCode.GetEnumDescription()),
            FLAdminErrorCode.UsernameAlreadyExists => controller.Conflict(errorCode.GetEnumDescription()),
            FLAdminErrorCode.IncorrectPassword => controller.Unauthorized(errorCode.GetEnumDescription()),
            FLAdminErrorCode.AccountElementTypeMismatch => controller.BadRequest(errorCode.GetEnumDescription()),
            FLAdminErrorCode.AccountAlreadyHasUsername => controller.Conflict(errorCode.GetEnumDescription()),
            FLAdminErrorCode.AccountFieldDoesNotExist => controller.NotFound(errorCode.GetEnumDescription()),
            FLAdminErrorCode.AccountIdAlreadyExists => controller.Conflict(errorCode.GetEnumDescription()),
            FLAdminErrorCode.AccountIsProtected => controller.Forbid(errorCode.GetEnumDescription()),
            FLAdminErrorCode.AccountFieldIsProtected => controller.Forbid(errorCode.GetEnumDescription()),
            FLAdminErrorCode.AccountFieldAlreadyExists => controller.Conflict(errorCode.GetEnumDescription()),
            FLAdminErrorCode.SuperAdminRoleIsProtected => controller.Forbid(errorCode.GetEnumDescription()),
            FLAdminErrorCode.CharacterAlreadyExists => controller.Conflict(errorCode.GetEnumDescription()),
            FLAdminErrorCode.CharacterIdIsNull => controller.BadRequest(errorCode.GetEnumDescription()),
            FLAdminErrorCode.CharacterNotFound => controller.NotFound(errorCode.GetEnumDescription()),
            FLAdminErrorCode.CharacterFieldAlreadyExists => controller.Conflict(errorCode.GetEnumDescription()),
            FLAdminErrorCode.CharacterFieldIsProtected => controller.Forbid(errorCode.GetEnumDescription()),
            FLAdminErrorCode.CharacterFieldDoesNotExist => controller.NotFound(errorCode.GetEnumDescription()),
            FLAdminErrorCode.CharacterElementTypeMismatch => controller.BadRequest(errorCode.GetEnumDescription()),
            FLAdminErrorCode.CharacterInvalidCharacter => controller.BadRequest(errorCode.GetEnumDescription()),
            FLAdminErrorCode.CharacterAccountError => controller.BadRequest(errorCode.GetEnumDescription()),
            FLAdminErrorCode.CharacterIsLoggedIn => controller.BadRequest(errorCode.GetEnumDescription()),
            FLAdminErrorCode.FLServerFailedToStart => new ObjectResult(StatusCodes.Status500InternalServerError),
            FLAdminErrorCode.FLHookFailedToStart => new ObjectResult(StatusCodes.Status500InternalServerError),
            


            _ => throw new ArgumentOutOfRangeException(nameof(errorCode), errorCode, null)
        };
    }
}