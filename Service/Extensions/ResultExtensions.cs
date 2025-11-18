using System.ComponentModel;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Error;
using Microsoft.AspNetCore.Http.HttpResults;
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

    public static IActionResult ParseError(this ErrorResult error, ControllerBase controller)
    {
        if (error.Errors.Count is 0)
        {
            var res = new ObjectResult(error)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            return res;
        }
        
        var firstError = error.Errors.FirstOrDefault();

        switch (firstError.ErrorCode)
        {
            case FLAdminErrorCode.Unknown:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return res;
            }
                break;
            case FLAdminErrorCode.RequestCancelled:
                return controller.BadRequest(error);
            case FLAdminErrorCode.HangfireFailure:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return res;
            }
                break;
            case FLAdminErrorCode.DatabaseError:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return res;
            }
                break;
            case FLAdminErrorCode.SessionNotStarted:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.CommandError:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.SessionAlreadyExists:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.SessionIdMismatch:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.AccountNotFound:
                return controller.NotFound(error);
                break;
            case FLAdminErrorCode.AccountIdIsNull:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.UsernameAlreadyExists:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.IncorrectPassword:
                return controller.Unauthorized(error);
                break;
            case FLAdminErrorCode.AccountElementTypeMismatch:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.AccountAlreadyHasUsername:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.AccountFieldDoesNotExist:
                return controller.NotFound(error);
                break;
            case FLAdminErrorCode.AccountIdAlreadyExists:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.AccountIsProtected:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return res;
            }
                break;
            case FLAdminErrorCode.AccountFieldIsProtected:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return res;
            }
                break;
            case FLAdminErrorCode.SuperAdminRoleIsProtected:
                return controller.Unauthorized(error);
                break;
            case FLAdminErrorCode.AccountFieldAlreadyExists:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.AccountTooManyCharacters:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.CharacterAlreadyExists:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.CharacterNameIsTaken:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.InvalidCharacter:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.CharacterIdIsNull:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.CharacterNotFound:
                return controller.NotFound(error);
                break;
            case FLAdminErrorCode.CharacterFieldAlreadyExists:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.CharacterFieldIsProtected:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return res;
            }
                break;
            case FLAdminErrorCode.CharacterFieldDoesNotExist:
                return controller.NotFound(error);
                break;
            case FLAdminErrorCode.CharacterElementTypeMismatch:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.CharacterInvalidCharacter:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.CharacterAccountError:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.CharacterIsLoggedIn:
                return controller.BadRequest(error);
                break;
            case FLAdminErrorCode.FLServerFailedToStart:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return res;
            }
                break;
            case FLAdminErrorCode.FLHookFailedToStart:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return res;
            }
                break;
            case FLAdminErrorCode.FLServerFailedToTerminate:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return res;
            }
                break;
            case FLAdminErrorCode.FLHookRequestTimeout:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return res;
            }
                break;
            case FLAdminErrorCode.FlHookHttpError:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return res;
            }
                break;
            case FLAdminErrorCode.FlHookFailedToRespond:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return res;
            }
                break;
            case FLAdminErrorCode.ServerAlreadyOnline:
            {
                var res = new ObjectResult(error)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                return res;
            }
                break;
            case FLAdminErrorCode.FileNotFound:
                return controller.NotFound(error);
                break;
            case FLAdminErrorCode.FileNotValidJson:
                return controller.BadRequest(error);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}