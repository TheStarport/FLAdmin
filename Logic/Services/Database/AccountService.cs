using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services.Auth;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace FlAdmin.Logic.Services.Database;

public class AccountService(IAccountDataAccess accountDataAccess, FlAdminConfig config, ILogger<AccountService> logger)
    : IAccountService
{
    
    public async Task<List<Account>> GetAllAccounts()
    {
        return await accountDataAccess.GetAccountsByFilter(account => true);
    }

    public List<Account> QueryAccounts(IQueryable<Account> query)
    {
        logger.LogError("Attempted usage of unimplemented function.");
        throw new NotImplementedException();
    }

    public async Task<Either<AccountError, Account>> GetAccountById(string id)
    {
        return await accountDataAccess.GetAccount(id);
    }

    public async Task<Option<AccountError>> CreateAccounts(params Account[] accounts)
    {
        return await accountDataAccess.CreateAccounts(accounts);
    }

    public async Task<Option<AccountError>> UpdateAccount(Account account)
    {
        return await accountDataAccess.UpdateAccount(account.ToBsonDocument());
    }

    public async Task<Option<AccountError>> DeleteAccounts(params string[] ids)
    {
        return await accountDataAccess.DeleteAccounts(ids);
    }

    public async Task<Option<AccountError>> UpdateFieldOnAccount<T>(string accountId, string name, T value)
    {
        return await accountDataAccess.UpdateFieldOnAccount(accountId, name, value);
    }

    public List<Account> GetOnlineAccounts()
    {
        //TODO: Requires the implementation of message queue talking with FLServer for this functionality.
        logger.LogError("Attempted usage of unimplemented function.");
        throw new NotImplementedException();
    }

    public async Task<Option<AccountError>> CreateWebMaster(LoginModel loginModel)
    {
        var name = loginModel.Username.Trim();

        var accountCheck = await accountDataAccess.GetAccountsByFilter(account => account.Username == name, 1, 1);
        if (accountCheck.Count != 0) return AccountError.UsernameAlreadyExists;

        var password = loginModel.Password.Trim();
        byte[]? salt = null;
        var hashedPass = PasswordHasher.GenerateSaltedHash(password, ref salt);
        var account = new Account
        {
            Id = "SuperAdmin",
            PasswordHash = hashedPass,
            Salt = salt,
            Username = name,
            WebRoles = []
        };
        await accountDataAccess.CreateAccounts(account);
        return new Option<AccountError>();
    }

    public async Task<Either<AccountError, Account>> GetAccountByUserName(string userName)
    {
        var accounts = await accountDataAccess.GetAccountsByFilter(account => account.Username == userName);
        if (accounts.Count == 0) return AccountError.AccountNotFound;

        if (accounts.Count != 1)
        {
            logger.LogError("There exists multiple accounts with the username of {username}", userName);
            return AccountError.DatabaseError;
        }

        return accounts[0];
    }

    public async Task<Either<AccountError, List<Account>>> GetAccountsActiveAfterDate(DateTimeOffset date, int page = 1,
        int pageSize = 100)
    {
        var accounts = await accountDataAccess.GetAccountsByFilter(x => x.LastOnline >= date, page, pageSize);

        if (accounts.Count == 0) return AccountError.AccountNotFound;

        return accounts;
    }


    public async Task<Option<AccountError>> SetUpAdminAccount(string accountId, LoginModel login)
    {
        var accountEnum = await accountDataAccess.GetAccount(accountId);
        var account = accountEnum.Match<Account>(
            Left: _ => null!,
            Right: val => val
        );
        if (account == null) return AccountError.AccountNotFound;


        var username = login.Username.Trim();
        //Make sure the username is unique. 
        var found = (await accountDataAccess.GetAccountsByFilter(acc => acc.Username == username)).Count is not 0;
        if (found)
        {
            logger.LogWarning(
                "Attempt to add username {} to account {} when the username exists on another account.", username,
                accountId);
            return AccountError.UsernameAlreadyExists;
        }

        //Make sure the account does not already have a username. 
        if (account.Username != null)
        {
            logger.LogWarning("There was an attempt to add the username {} to account {} when it already has one.",
                username, account.Id);
            return AccountError.AccountAlreadyHasUsername;
        }

        var password = login.Password.Trim();
        byte[]? salt = null;
        var hashedPass = PasswordHasher.GenerateSaltedHash(password, ref salt);

        account.PasswordHash = hashedPass;
        account.Salt = salt;
        account.Username = username;

        await accountDataAccess.UpdateAccount(account.ToBsonDocument());
        return new Option<AccountError>();
    }

    public async Task<Option<AccountError>> ChangePassword(LoginModel login, string newPassword)
    {
        var username = login.Username.Trim();
        var foundAccounts = await accountDataAccess.GetAccountsByFilter(acc => acc.Username == username);
        if (foundAccounts.Count == 0) return AccountError.AccountNotFound;

        if (foundAccounts.Count != 1)
        {
            logger.LogError("There exists multiple accounts with the username of {username}.", username);
            return AccountError.DatabaseError;
        }

        var account = foundAccounts[0];
        if (account.PasswordHash is null || account.Salt is null) return AccountError.FieldDoesNotExist;

        //Make sure the old password is correct before changing it.
        var password = login.Password.Trim();
        var verified = PasswordHasher.VerifyPassword(password, account.PasswordHash, account.Salt);
        if (!verified) return AccountError.IncorrectPassword;
        byte[]? salt = null;
        var hashedPass = PasswordHasher.GenerateSaltedHash(password.Trim(), ref salt);
        account.PasswordHash = hashedPass;
        account.Salt = salt;
        return await accountDataAccess.UpdateAccount(account.ToBsonDocument());
    }

    public async Task<Option<AccountError>> BanAccount(string id, TimeSpan? duration)
    {
        duration ??= TimeSpan.FromDays(109500);
        var doc = new BsonDocument
        {
            {"_id", id},
            {"duration", BsonValue.Create(duration.Value)}
        };
        return await accountDataAccess.UpdateAccount(doc);
    }

    public async Task<Option<AccountError>> UnBanAccount(string id)
    {
        var doc = new BsonDocument
        {
            {"_id", id},
            {"duration", BsonValue.Create(null)}
        };
        return await accountDataAccess.UpdateAccount(doc);
    }

    public async Task<Option<AccountError>> RemoveRolesFromAccount(string id, List<Role> roles)
    {
        var set = roles.Select(x => x.ToString()).ToHashSet();
        var accountEnum = await accountDataAccess.GetAccount(id);
        var acc = accountEnum.Match<Account>(
            Left: _ => null!,
            Right: val => val
        );
        if (acc == null) return AccountError.AccountNotFound;
        acc.WebRoles.ExceptWith(set);
        return await accountDataAccess.UpdateFieldOnAccount(id, "webRoles", acc.WebRoles);
    }

    public async Task<Option<AccountError>> AddRolesToAccount(string id, List<Role> roles)
    {
        var set = roles.Select(x => x.ToString()).ToHashSet();
        var accountEnum = await accountDataAccess.GetAccount(id);
        var acc = accountEnum.Match<Account>(
            Left: _ => null!,
            Right: val => val
        );
        if (acc == null) return AccountError.AccountNotFound;
        acc.WebRoles.UnionWith(set);
        return await accountDataAccess.UpdateFieldOnAccount(id, "webRoles", acc.WebRoles);
    }
}