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
    
    /// <summary>
    /// Gets all accounts on the database with pagination.
    /// </summary>
    /// <param name="pageCount">The page that will be grabbed</param>
    /// <param name="pageSize"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("getaccounts")]
    public async Task<IActionResult> GetAccounts([FromQuery] int pageCount, [FromQuery] int pageSize,
        CancellationToken token)
    {
        var accounts = await accountService.GetAccounts(token, pageCount, pageSize);
        var accountModels = new List<AccountModel>();
        accounts.ForEach(account => accountModels.Add(account.ToModel()));
        if (accountModels.Count is 0) return NotFound("No accounts were found,");

        return Ok(accountModels);
    }

    
    /// <summary>
    /// Gets an account by a specified AccountID String used by FLServer
    /// </summary>
    /// <param name="id">The id for the account.</param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAccountById(string id, CancellationToken token)
    {
        var account = await accountService.GetAccountById(token, id);

        var accountModel = account.Match<Either<FLAdminError, AccountModel>>(
            Left: err => err,
            Right: val => val.ToModel());

        return accountModel.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val)
        );
    }

    /// <summary>
    /// Gets a paginated list of accounts that have been logged into on FLServer within a specified date.
    /// </summary>
    /// <param name="date">The cut-off date that will filter for any accounts logged in after this date.</param>
    /// <param name="pageNumber">The page that will be grabbed</param>
    /// <param name="pageSize"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("activeafterdate")]
    public async Task<IActionResult> GetAccountsActiveAfterDate([FromQuery] DateTimeOffset date,
        [FromQuery] int pageNumber, [FromQuery] int pageSize, CancellationToken token)
    {
        var accounts = await accountService.GetAccountsActiveAfterDate(date, token, pageNumber, pageSize);

        var accountModels = accounts.Match<Either<FLAdminError, List<AccountModel>>>(
            Left: err => err,
            Right: accs => accs.Select(a => a.ToModel()).ToList());

        return accountModels.Match<IActionResult>(
            Left: err => err.ParseError(this),
            Right: val => Ok(val)
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">List of accounts to be deleted.</param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAccounts([FromBody] string[] id, CancellationToken token)
    {
        if (id.Length is 0) return BadRequest();
        var res = await accountService.DeleteAccounts(token, id);

        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok("Account(s) successfully deleted.")
        );
    }

    /// <summary>
    /// Updates account via a find and replace, will preserve certain information such as password and web account.
    /// </summary>
    /// <param name="accountModel">The new account to be updated to.</param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("update")]
    [AdminAuthorize(Role.SuperAdmin)]
    public async Task<IActionResult> UpdateAccount([FromBody] AccountModel accountModel, CancellationToken token)
    {
        var account = accountModel.ToDatabaseAccount();

        //Since our external model is different from the database we need to check if any old information is on it and
        // preserve it if it exists (such as password and salts. and username)
        _ = (await accountService.GetAccountById(token, account.Id)).Match(
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

        await accountService.UpdateAccount(token, account);
        return Ok($"Account {account.Id} updated successfully");
    }


    /// <summary>
    /// Bans account provided.
    /// </summary>
    /// <param name="bans">Ban payload of ID and duration, no duration will default to effectively perma ban.</param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("ban")]
    public async Task<IActionResult> BanAccounts([FromBody] BanAccountsPayload bans, CancellationToken token)
    {
        var concatOkMessage = "";
        foreach (var ban in bans.Bans)
        {
            var okMessage = ban.Duration is not null
                ? $"Account {ban.AccountId} banned for {ban.Duration}"
                : $"Account {ban.AccountId} has been banned.";
            concatOkMessage += okMessage;
        }


        var res = await accountService.BanAccounts(bans.Bans
            .Select(ban => new Tuple<string, TimeSpan?>(ban.AccountId, ban.Duration)).ToList(), token);


        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok(concatOkMessage)
        );
    }

    
    /// <summary>
    /// Unbans the accounts of the provided ids.
    /// </summary>
    /// <param name="accountIds"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPatch("unban")]
    public async Task<IActionResult> UnbanAccount([FromQuery] string[] accountIds, CancellationToken token)
    {
        var res = await accountService.UnBanAccounts(accountIds, token);
        return res.Match<IActionResult>(
            err => err.ParseError(this),
            Ok($"Accounts {accountIds} has been unbanned.")
        );
    }
}