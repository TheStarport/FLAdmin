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
        return new BadRequestObjectResult(error);
    }
}