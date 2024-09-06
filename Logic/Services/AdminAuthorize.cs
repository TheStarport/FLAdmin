using Microsoft.AspNetCore.Authorization;
using FlAdmin.Common.Models.Auth;

namespace FlAdmin.Logic.Services;

public class AdminAuthorizeAttribute : AuthorizeAttribute
{
    public AdminAuthorizeAttribute(params Role[] roles) => Roles = $"{string.Join(",", roles.Distinct().Select(x => x.ToString()))}, {Role.SuperAdmin}";
}