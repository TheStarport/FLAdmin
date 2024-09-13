using System.ComponentModel;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Common.Extensions;

public static class ResultExtensions
{
    public static string GetEnumDescription(this Enum value)
    {
        return value.GetType()
            .GetField(value.ToString())
            ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .SingleOrDefault() is not DescriptionAttribute attribute ? value.ToString() : attribute.Description;
    }
    
    
    public static IActionResult ToResponse<L, R>(this Either<L, R> either, ControllerBase controller) where L : Enum
    {
        return either.Match<IActionResult>(
            Left: error => controller.BadRequest(error),
            Right: value => controller.Ok(value)
        );
    }

    public static IActionResult ToResponse<T>(this Option<T> option, ControllerBase controller, string okResponse) where T : Enum
    {
        return option.Match<IActionResult>(
            Some: error => controller.BadRequest(error),
            None: controller.Ok(okResponse)
        );
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
            _ => throw new ArgumentOutOfRangeException(nameof(accountError), accountError, null)
        };
    }
    
}