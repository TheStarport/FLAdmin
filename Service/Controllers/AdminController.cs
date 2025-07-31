using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Payloads;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IDatabaseAccess database, IAccountService accountService) : ControllerBase
{
    [HttpPost]
    [Route("startatabasesession")]
    [AdminAuthorize(Role.Database)]
    public async Task<IActionResult> StartDatabaseSession()
    {
        var res = await database.StartSession();

        return res.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: guid => Ok(guid));
    }

    [HttpPost]
    [Route("querydatabase")]
    [AdminAuthorize(Role.Database)]
    public async Task<IActionResult> DatabaseQuery([FromBody] CommandPayload command)
    {
        var res  = await database.SubmitQuery(command.Command, command.SessionId);

        return res.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: doc => Ok(doc)
        );
    }

    [HttpPost]
    [Route("enddatabasesession")]
    [AdminAuthorize(Role.Database)]
    public async Task<IActionResult> EndDatabaseSession([FromQuery] bool commit)
    {
        var res = await database.EndSession(commit);
        return res.Match<IActionResult>(
            Some: err => err.ParseError(this),
            None: () => Ok()
        );
    }
    
    [HttpPatch("addusername")]
    [AdminAuthorize(Role.ManageAdmins)]
    public async Task<IActionResult> AddUsernameToAccount([FromBody] LoginModel login, [FromQuery] string accountId)
    {
        if (login?.Username is null || login?.Password is null || login.Password.Trim().Length is 0 ||
            login.Username.Trim().Length is 0)
            return BadRequest();

        var res = await accountService.SetUpAdminAccount(accountId, login);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Username and password set successfully.")
        );
    }
    
    [HttpPatch("addroles")]
    [AdminAuthorize(Role.ManageRoles)]
    public async Task<IActionResult> AddRolesToAccount([FromBody] RolePayload rolePayload)
    {
        var roles = new List<Role>();
        foreach (var roleStr in rolePayload.Roles)
            if (Enum.TryParse(roleStr, out Role role))
            {
                if (role is Role.SuperAdmin)
                    continue;
                roles.Add(role);
            }

        if (roles.Count is 0) return BadRequest("No valid Roles were supplied");
        var res = await accountService.AddRolesToAccount(rolePayload.AccountId.Trim(), roles);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Role(s) added to {rolePayload.AccountId} successfully.")
        );
    }
    
    [HttpPatch("removeroles")]
    [AdminAuthorize(Role.ManageRoles)]
    public async Task<IActionResult> RemoveRolesFromAccount([FromBody] RolePayload rolePayload)
    {
        var roles = new List<Role>();
        foreach (var roleStr in rolePayload.Roles)
            if (Enum.TryParse(roleStr, out Role role))
            {
                if (role is Role.SuperAdmin)
                    continue;
                roles.Add(role);
            }

        if (roles.Count is 0) return BadRequest("No valid Roles were supplied");

        var res = await accountService.RemoveRolesFromAccount(rolePayload.AccountId.Trim(), roles);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Role(s) removed from account {rolePayload.AccountId} successfully.")
        );
    }

    [HttpPatch("updatepassword")]
    public async Task<IActionResult> UpdatePassword([FromBody] LoginModel login, [FromQuery] string newPassword)
    {
        if (newPassword.Trim().Length is 0) return BadRequest();
        var res = await accountService.ChangePassword(login, newPassword);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Password changed successfully.")
        );
    }
}