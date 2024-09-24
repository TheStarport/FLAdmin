using FlAdmin.Common.Extensions;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Controllers;

[ApiController]
[Route("api/accounts")]
[AdminAuthorize(Role.ManageAccounts)]
public class AccountController(IAccountService accountService) : ControllerBase
{
    [HttpPatch("addroles")]
    [AdminAuthorize(Role.ManageRoles)]
    public async Task<IActionResult> AddRolesToAccount([FromQuery] string accountId, string[] rolesStr)
    {
        var roles = new List<Role>();
        foreach (var roleStr in rolesStr)
            if (Enum.TryParse(roleStr, out Role role))
            {
                if (role == Role.SuperAdmin)
                    continue;
                roles.Add(role);
            }

        if (roles.Count is 0) return BadRequest("No valid Roles were supplied");
        var res = await accountService.AddRolesToAccount(accountId.Trim(), roles);
        return res.Match<IActionResult>(
            Some: err => err.ParseAccountError(this),
            None: Ok($"Role(s) added to {accountId} successfully.")
        );
    }

    [HttpPatch("removerolls")]
    [AdminAuthorize(Role.ManageRoles)]
    public async Task<IActionResult> RemoveRolesFromAccount([FromQuery] string accountId, string[] rolesStr)
    {
        var roles = new List<Role>();
        foreach (var roleStr in rolesStr)
            if (Enum.TryParse(roleStr, out Role role))
            {
                if (role == Role.SuperAdmin)
                    continue;
                roles.Add(role);
            }

        if (roles.Count is 0) return BadRequest("No valid Roles were supplied");
        
        
        var res = await accountService.RemoveRolesFromAccount(accountId.Trim(), roles);
        return res.Match<IActionResult>(
            Some: err => err.ParseAccountError(this),
            None: Ok($"Role(s) removed from account {accountId} successfully.")
        );
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllAccounts()
    {
        var accounts = await accountService.GetAllAccounts();
        var accountModels = new List<AccountModel>();
        accounts.ForEach(account => accountModels.Add(account.ToModel()));
        if (accountModels.Count is 0)
        {
            return NotFound("No accounts were found,");
        }

        return Ok(accountModels);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(string id)
    {
        var account = await accountService.GetAccountById(id);

        var accountModel = account.Match<Either<AccountError, AccountModel>>(
            Left: err => err,
            Right: val => val.ToModel());

        return accountModel.Match<IActionResult>(
            Left: err => err.ParseAccountError(this),
            Right: val => Ok(val)
        );
    }

    [HttpGet("activeafterdate")]
    public async Task<IActionResult> GetAccountsActiveAfterDate([FromQuery] DateTimeOffset date)
    {
        var accounts = await accountService.GetAccountsActiveAfterDate(date);

        var accountModels = accounts.Match<Either<AccountError, List<AccountModel>>>(
            Left: err => err,
            Right: accs => accs.Select(a => a.ToModel()).ToList());

        return accountModels.Match<IActionResult>(
            Left: err => err.ParseAccountError(this),
            Right: val => Ok(val)
        );
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAccounts([FromQuery] string[] id)
    {
        if (id.Length is 0) return BadRequest();
        var res = await accountService.DeleteAccounts(id);

        return res.Match<IActionResult>(
            Some: err => err.ParseAccountError(this),
            None: Ok("Account(s) successfully deleted.")
        );
    }

    [HttpPatch("update")]
    public async Task<IActionResult> UpdateAccount([FromBody] AccountModel accountModel)
    {
        var account = accountModel.ToDatabaseAccount();

        //Since our external model is different from the database we need to check if any old information is on it and
        // preserve it if it exists (such as password and salts. and username)
        _ = (await accountService.GetAccountById(account.Id)).Match(
            Left: _ => new Option<Account>(),
            Right: val =>
            {
                if (val.Username is not null)
                {
                    account.Username = val.Username;
                }

                if (val.PasswordHash is not null)
                {
                    account.PasswordHash = val.PasswordHash;
                }

                if (val.Salt is not null)
                {
                    account.Salt = val.Salt;
                }

                account.Extra = val.Extra;
                return val;
            }
        );

        await accountService.UpdateAccount(account);
        return Ok($"Account {account.Id} updated successfully");
    }

    [HttpPatch("addusername")]
    [AdminAuthorize(Role.ManageAdmins)]
    public async Task<IActionResult> AddUsernameToAccount([FromQuery] string accountId, [FromQuery] LoginModel login)
    {
        if (login?.Username is null || login?.Password is null || login.Password.Trim().Length is 0 ||
            login.Username.Trim().Length is 0)
        {
            return BadRequest();
        }

        var res = await accountService.SetUpAdminAccount(accountId, login);

        return res.Match<IActionResult>(
            Some: err => err.ParseAccountError(this),
            None: Ok("Username and password set successfully.")
        );
    }

    [HttpPatch("updatepassword")]
    public async Task<IActionResult> UpdatePassword([FromQuery] LoginModel login, [FromQuery] string newPassword)
    {
        if (newPassword.Trim().Length is 0) return BadRequest();
        var res = await accountService.ChangePassword(login, newPassword);

        return res.Match<IActionResult>(
            Some: err => err.ParseAccountError(this),
            None: Ok("Password changed successfully.")
        );
    }

    [HttpPatch("ban")]
    public async Task<IActionResult> BanAccount([FromQuery] string accountId, [FromQuery] TimeSpan? duration)
    {
        var okMessage = duration is not null
            ? $"Account {accountId} banned for {duration}"
            : $"Account {accountId} has been banned.";
        var res = await accountService.BanAccount(accountId, duration);
        return res.Match<IActionResult>(
            Some: err => err.ParseAccountError(this),
            None: Ok(okMessage)
        );
    }

    [HttpPatch("unban")]
    public async Task<IActionResult> UnbanAccount([FromQuery] string accountId)
    {
        var res = await accountService.UnBanAccount(accountId);
        return res.Match<IActionResult>(
            Some: err => err.ParseAccountError(this),
            None: Ok($"Account {accountId} has been unbanned.")
        );
    }
    
    
    
}