using FlAdmin.Common.Extensions;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Models.Payloads;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services;
using FlAdmin.Service.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Mvc;

namespace FlAdmin.Service.Controllers;

[ApiController]
[Route("api/accounts")]
[AdminAuthorize(Role.ManageAccounts)]
public class AccountController(IAccountService accountService) : ControllerBase
{

    [HttpGet("getaccounts")]
    public async Task<IActionResult> GetAccounts([FromQuery] int pageCount, [FromQuery] int pageSize)
    {
        var accounts = await accountService.GetAccounts(pageCount, pageSize);
        var accountModels = new List<AccountModel>();
        accounts.ForEach(account => accountModels.Add(account.ToModel()));
        if (accountModels.Count is 0) return NotFound("No accounts were found,");

        return Ok(accountModels);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(string id)
    {
        var account = await accountService.GetAccountById(id);

        var accountModel = account.Match<Either<FLAdminError, AccountModel>>(
            Left: err => err,
            Right: val => val.ToModel());

        return accountModel.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val)
        );
    }

    [HttpGet("activeafterdate")]
    public async Task<IActionResult> GetAccountsActiveAfterDate([FromQuery] DateTimeOffset date,
        [FromQuery] int pageNumber, [FromQuery] int pageSize)
    {
        var accounts = await accountService.GetAccountsActiveAfterDate(date, pageNumber, pageSize);

        var accountModels = accounts.Match<Either<FLAdminError, List<AccountModel>>>(
            Left: err => err,
            Right: accs => accs.Select(a => a.ToModel()).ToList());

        return accountModels.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val)
        );
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAccounts([FromBody] string[] id)
    {
        if (id.Length is 0) return BadRequest();
        var res = await accountService.DeleteAccounts(id);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Account(s) successfully deleted.")
        );
    }
    
    [HttpPatch("update")]
    [AdminAuthorize(Role.SuperAdmin)]
    public async Task<IActionResult> UpdateAccount([FromBody] AccountModel accountModel)
    {
        var account = accountModel.ToDatabaseAccount();
        
        //Since our external model is different from the database we need to check if any old information is on it and
        // preserve it if it exists (such as password and salts. and username)
        _ = (await accountService.GetAccountById(account.Id)).Match(
            Left: _ => new Option<Account>(),
            Right: val =>
            {
                if (val.Username is not null) account.Username = val.Username;

                if (val.PasswordHash is not null) account.PasswordHash = val.PasswordHash;

                if (val.Salt is not null) account.Salt = val.Salt;

                account.Extra = val.Extra;
                return val;
            }
        );

        await accountService.UpdateAccount(account);
        return Ok($"Account {account.Id} updated successfully");
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

    [HttpPatch("ban")]
    public async Task<IActionResult> BanAccount([FromQuery] string accountId, [FromQuery] TimeSpan? duration)
    {
        var okMessage = duration is not null
            ? $"Account {accountId} banned for {duration}"
            : $"Account {accountId} has been banned.";
        var res = await accountService.BanAccount(accountId, duration);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok(okMessage)
        );
    }

    [HttpPatch("unban")]
    public async Task<IActionResult> UnbanAccount([FromQuery] string accountId)
    {
        var res = await accountService.UnBanAccount(accountId);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Account {accountId} has been unbanned.")
        );
    }
}