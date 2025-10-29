using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Payloads;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IDatabaseAccess database, IAccountService accountService, IConfigService configService)
    : ControllerBase
{
    /// <summary>
    /// Starts a database session with mongo for database queries. Caution: This is a stateful system. 
    /// </summary>
    /// <param name="token"></param>
    /// <returns>Returns a unique ID of the session that is generated.</returns>
    [HttpPost("startatabasesession")]
    [AdminAuthorize(Role.Database)]
    public async Task<IActionResult> StartDatabaseSession(CancellationToken token)
    {
        var res = await database.StartSession();

        return res.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: guid => Ok(guid));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command">A payload of the session ID and a command as a BSON Document, See MongoDB documentation for queries.  </param>
    /// <param name="token"></param>
    /// <returns>Returns a BSON doc that is the result of the query. </returns>
    [HttpPost("querydatabase")]
    [AdminAuthorize(Role.Database)]
    public async Task<IActionResult> DatabaseQuery([FromBody] CommandPayload command, CancellationToken token)
    {
        var res = await database.SubmitQuery(command.Command, command.SessionId);

        return res.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: doc => Ok(doc)
        );
    }

    
    /// <summary>
    /// Ends the session provided.
    /// </summary>
    /// <param name="commit">Tells the database to either commit the changes provided during the session or discard them. (True: Commit, False: Discard)</param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPost("enddatabasesession")]
    [AdminAuthorize(Role.Database)]
    public async Task<IActionResult> EndDatabaseSession([FromQuery] bool commit, CancellationToken token)
    {
        var res = await database.EndSession(commit);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            () => Ok()
        );
    }

    /// <summary>
    /// Adds a username and password to a provided account. 
    /// </summary>
    /// <param name="login">Login payload of username and password.</param>
    /// <param name="accountId">Account that is being set up with a username and password.</param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("addusername")]
    [AdminAuthorize(Role.ManageAdmins)]
    public async Task<IActionResult> AddUsernameToAccount([FromBody] LoginModel login, [FromQuery] string accountId,
        CancellationToken token)
    {
        if (login?.Username is null || login?.Password is null || login.Password.Trim().Length is 0 ||
            login.Username.Trim().Length is 0)
            return BadRequest();

        var res = await accountService.SetUpAdminAccount(accountId, login, token);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Username and password set successfully.")
        );
    }

    
    /// <summary>
    /// Adds roles to a provided account.
    /// </summary>
    /// <param name="rolePayload">Payload of the account and a list of roles to add. </param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("addroles")]
    [AdminAuthorize(Role.ManageRoles)]
    public async Task<IActionResult> AddRolesToAccount([FromBody] RolePayload rolePayload, CancellationToken token)
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
        var res = await accountService.AddRolesToAccount(rolePayload.AccountId.Trim(), roles, token);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Role(s) added to {rolePayload.AccountId} successfully.")
        );
    }

    /// <summary>
    /// Removes roles from a provided account
    /// </summary>
    /// <param name="rolePayload">Payload of the account and a list of roles to remove.</param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("removeroles")]
    [AdminAuthorize(Role.ManageRoles)]
    public async Task<IActionResult> RemoveRolesFromAccount([FromBody] RolePayload rolePayload, CancellationToken token)
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

        var res = await accountService.RemoveRolesFromAccount(rolePayload.AccountId.Trim(), roles, token);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Role(s) removed from account {rolePayload.AccountId} successfully.")
        );
    }

    
    /// <summary>
    /// </summary>
    /// <param name="login"></param>
    /// <param name="newPassword"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("updatepassword")]
    public async Task<IActionResult> UpdatePassword([FromBody] LoginModel login, [FromQuery] string newPassword,
        CancellationToken token)
    {
        if (newPassword.Trim().Length is 0) return BadRequest();
        var res = await accountService.ChangePassword(login, newPassword, token);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Password changed successfully.")
        );
    }

    /// <summary>
    /// Resets FLAdmins config to its default values. 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("resetconfig")]
    [AdminAuthorize(Role.ManageServer)]
    public async Task<IActionResult> ResetFlAdminConfig(CancellationToken token)
    {
        var res = await configService.GenerateDefaultFlAdminConfig(token);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Config changed successfully.")
        );
    }
}