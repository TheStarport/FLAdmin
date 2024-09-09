using FlAdmin.Common.Extensions;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Controllers;

[ApiController]
[Route("api/accounts")]
[AdminAuthorize(Role.ManageAccounts)]
public class AccountController(IAccountService accountService) : ControllerBase
{
    [HttpPatch("addroles")]
    [AdminAuthorize(Role.ManageRoles)]
    public async Task<IActionResult> AddRolesToAccount([FromQuery] string id, string[] rolesStr)
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

        await accountService.AddRolesToAccount(id, roles);
        return Ok("Roles successfully added to account");
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllAccounts()
    {
        var accounts = await accountService.GetAllAccounts();
        var accountModels = new List<AccountModel>();
        accounts.ForEach(account => accountModels.Add(account.ToAccountModel()));
        return Ok(accountModels);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(string id)
    {
        var account = await accountService.GetAccountById(id);
        if (account is null)
            return NotFound();
        return Ok(account.ToAccountModel());
    }

    [HttpGet("activeafterdate")]
    public async Task<IActionResult> GetAccountsActiveAfterDate([FromQuery] DateTimeOffset date)
    {
        var accounts = await accountService.GetAccountsActiveAfterDate(date);
        if (accounts.Count == 0) return NotFound();
        var accountModels = new List<AccountModel>();
        accounts.ForEach(account => accountModels.Add(account.ToAccountModel()));
        return Ok(accountModels);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAccounts([FromQuery] string[] id)
    {
        if (id.Length is 0) return BadRequest();
        await accountService.DeleteAccounts(id);

        return Ok("Account(s) deleted successfully");
    }

    [HttpPatch("update")]
    public async Task<IActionResult> UpdateAccount([FromBody] AccountModel accountModel)
    {
        var account = accountModel.ToDatabaseAccount();

        //Since our external model is different from the database we need to check if any old information is on it and
        // preserve it if it exists (such as password and salts. and username)
        var dbAccount = await accountService.GetAccountById(account.Id);

        if (dbAccount is not null)
        {
            if (dbAccount.Username is not null)
                account.Username = dbAccount.Username;
            if (dbAccount.PasswordHash is not null)
                account.PasswordHash = dbAccount.PasswordHash;
            if (dbAccount.Salt is not null)
                account.Salt = dbAccount.Salt;
        }

        await accountService.UpdateAccount(account);
        return Ok("Account updated successfully");
    }
}