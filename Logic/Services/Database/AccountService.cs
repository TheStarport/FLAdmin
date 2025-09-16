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
    public async Task<List<Account>> GetAccounts(CancellationToken token, int pageCount, int pageSize)
    {
        return await accountDataAccess.GetAccountsByFilter(account => true, token, pageCount, pageSize);
    }

    public async Task<Either<FLAdminError, Account>> GetAccountById(CancellationToken token, string id)
    {
        return await accountDataAccess.GetAccount(id, token);
    }

    public async Task<Option<FLAdminError>> CreateAccounts(CancellationToken token, params Account[] accounts)
    {
        return await accountDataAccess.CreateAccounts(token, accounts);
    }

    public async Task<Option<FLAdminError>> UpdateAccount(CancellationToken token, Account account)
    {
        var acc = accountDataAccess.GetAccount(account.Id, token);


        return await accountDataAccess.UpdateAccount(account.ToBsonDocument(), token);
    }

    public async Task<Option<FLAdminError>> DeleteAccounts(CancellationToken token, params string[] ids)
    {
        if (ids.Contains(config.SuperAdminName)) return FLAdminError.AccountIsProtected;


        return await accountDataAccess.DeleteAccounts(token, ids);
    }

    public async Task<Option<FLAdminError>> UpdateFieldOnAccount<T>(CancellationToken token, string accountId,
        string name, T value)
    {
        if (name is "WebRoles" or "GameRoles") return FLAdminError.AccountFieldIsProtected;

        return await accountDataAccess.UpdateFieldOnAccount(accountId, name, value, token);
    }

    public async Task<Option<FLAdminError>> CreateWebMaster(CancellationToken token, LoginModel loginModel)
    {
        var name = loginModel.Username.Trim();

        var accountCheck = await accountDataAccess.GetAccountsByFilter(account => account.Username == name, token, 1, 1);
        if (accountCheck.Count != 0) return FLAdminError.UsernameAlreadyExists;

        var password = loginModel.Password.Trim();
        byte[]? salt = null;
        var hashedPass = PasswordHasher.GenerateSaltedHash(password, ref salt);


        var account = new Account
        {
            Id = config.SuperAdminName,
            PasswordHash = hashedPass,
            Salt = salt,
            Username = name,
            WebRoles = ["SuperAdmin"]
        };
        await accountDataAccess.CreateAccounts(token, account);
        return new Option<FLAdminError>();
    }

    public async Task<Either<FLAdminError, Account>> GetAccountByUserName(CancellationToken token, string userName)
    {
        var accounts = await accountDataAccess.GetAccountsByFilter(account => account.Username == userName, token);
        if (accounts.Count == 0) return FLAdminError.AccountNotFound;

        if (accounts.Count != 1)
        {
            logger.LogError("There exists multiple accounts with the username of {username}", userName);
            return FLAdminError.DatabaseError;
        }

        return accounts[0];
    }

    public async Task<Either<FLAdminError, List<Account>>> GetAccountsActiveAfterDate(DateTimeOffset date,
        CancellationToken token,
        int page = 1,
        int pageSize = 100)
    {
        var accounts = await accountDataAccess.GetAccountsByFilter(x => x.LastOnline >= date, token, page, pageSize);

        if (accounts.Count == 0) return FLAdminError.AccountNotFound;

        return accounts;
    }


    public async Task<Option<FLAdminError>> SetUpAdminAccount(string accountId,
        LoginModel login, CancellationToken token)
    {
        var accountEnum = await accountDataAccess.GetAccount(accountId, token);
        var account = accountEnum.Match<Account>(
            Left: _ => null!,
            Right: val => val
        );
        if (account == null) return FLAdminError.AccountNotFound;


        var username = login.Username.Trim();
        //Make sure the username is unique. 
        var found = (await accountDataAccess.GetAccountsByFilter(acc => acc.Username == username, token)).Count is not 0;
        if (found)
        {
            logger.LogWarning(
                "Attempt to add username {} to account {} when the username exists on another account.", username,
                accountId);
            return FLAdminError.UsernameAlreadyExists;
        }

        //Make sure the account does not already have a username. 
        if (account.Username != null)
        {
            logger.LogWarning("There was an attempt to add the username {} to account {} when it already has one.",
                username, account.Id);
            return FLAdminError.AccountAlreadyHasUsername;
        }

        var password = login.Password.Trim();
        byte[]? salt = null;
        var hashedPass = PasswordHasher.GenerateSaltedHash(password, ref salt);

        account.PasswordHash = hashedPass;
        account.Salt = salt;
        account.Username = username;

        await accountDataAccess.UpdateAccount(account.ToBsonDocument(), token);
        return new Option<FLAdminError>();
    }

    public async Task<Option<FLAdminError>> ChangePassword(LoginModel login,
        string newPassword, CancellationToken token)
    {
        var username = login.Username.Trim();
        var foundAccounts = await accountDataAccess.GetAccountsByFilter(acc => acc.Username == username, token);
        if (foundAccounts.Count == 0) return FLAdminError.AccountNotFound;

        if (foundAccounts.Count != 1)
        {
            logger.LogError("There exists multiple accounts with the username of {username}.", username);
            return FLAdminError.DatabaseError;
        }

        var account = foundAccounts[0];
        if (account.PasswordHash is null || account.Salt is null) return FLAdminError.AccountFieldDoesNotExist;

        //Make sure the old password is correct before changing it.
        var password = login.Password.Trim();
        var verified = PasswordHasher.VerifyPassword(password, account.PasswordHash, account.Salt);
        if (!verified) return FLAdminError.IncorrectPassword;
        byte[]? salt = null;
        var hashedPass = PasswordHasher.GenerateSaltedHash(password.Trim(), ref salt);
        account.PasswordHash = hashedPass;
        account.Salt = salt;
        return await accountDataAccess.UpdateAccount(account.ToBsonDocument(), token);
    }

    public async Task<Option<FLAdminError>> BanAccounts(List<Tuple<string, TimeSpan?>> bans, CancellationToken token)
    {
        foreach (var ban in bans)
        {
            //entering no duration means it's a permaban so we'll just ban them for 200 years, effectively a perma. 
            var banDuration = ban.Item2 ?? TimeSpan.FromDays(80000);

            var doc = new BsonDocument
            {
                { "_id", ban.Item1 },
                { "duration", BsonValue.Create(banDuration) }
            };
            var ret = await accountDataAccess.UpdateAccount(doc, token);

            //TODO:When error handling is reworked this should catch each individual error and still attempt to ban every item instead of quitting on first error.
            if (ret.IsSome) return ret;
        }

        return Option<FLAdminError>.None;
    }

    public async Task<Option<FLAdminError>> UnBanAccounts(string[] ids, CancellationToken token)
    {
        foreach (var id in ids)
        {
            var doc = new BsonDocument
            {
                { "_id", id },
                { "duration", BsonValue.Create(null) }
            };
            var ret = await accountDataAccess.UpdateAccount(doc, token);

            //TODO:When error handling is reworked this should catch each individual error and still attempt to ban every item instead of quitting on first error.
            if (ret.IsSome) return ret;
        }

        return Option<FLAdminError>.None;
    }

    public async Task<Option<FLAdminError>> RemoveRolesFromAccount(string id, List<Role> roles, CancellationToken token)
    {
        if (roles.Contains(Role.SuperAdmin)) return FLAdminError.SuperAdminRoleIsProtected;


        var set = roles.Select(x => x.ToString()).ToHashSet();
        var accountEnum = await accountDataAccess.GetAccount(id, token);
        var acc = accountEnum.Match<Account>(
            Left: _ => null!,
            Right: val => val
        );
        if (acc == null) return FLAdminError.AccountNotFound;
        acc.WebRoles.ExceptWith(set);
        return await accountDataAccess.UpdateFieldOnAccount(id, "webRoles", acc.WebRoles, token);
    }

    public async Task<Option<FLAdminError>> AddRolesToAccount(string id, List<Role> roles, CancellationToken token)
    {
        if (roles.Contains(Role.SuperAdmin)) return FLAdminError.SuperAdminRoleIsProtected;

        var set = roles.Select(x => x.ToString()).ToHashSet();
        var accountEnum = await accountDataAccess.GetAccount(id, token);
        var acc = accountEnum.Match<Account>(
            Left: _ => null!,
            Right: val => val
        );
        if (acc == null) return FLAdminError.AccountNotFound;
        acc.WebRoles.UnionWith(set);
        return await accountDataAccess.UpdateFieldOnAccount(id, "webRoles", acc.WebRoles, token);
    }
}