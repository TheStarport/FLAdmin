using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Extensions;
using FlAdmin.Common.Models;
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

    public async Task<Either<ErrorResult, Account>> GetAccountById(CancellationToken token, string id)
    {
        return await accountDataAccess.GetAccount(id, token);
    }

    public async Task<Option<ErrorResult>> CreateAccounts(CancellationToken token, params Account[] accounts)
    {
        return await accountDataAccess.CreateAccounts(token, accounts);
    }

    public async Task<Option<ErrorResult>> UpdateAccount(CancellationToken token, Account account)
    {
        var acc = accountDataAccess.GetAccount(account.Id, token);


        return await accountDataAccess.UpdateAccount(account.ToBsonDocument(), token);
    }

    public async Task<Option<ErrorResult>> DeleteAccounts(CancellationToken token, params string[] ids)
    {
        if (ids.Contains(config.SuperAdminName))
        {
            return new ErrorResult(FLAdminErrorCode.AccountIsProtected,
                "Superadmin cannot be deleted.");
        }

        return await accountDataAccess.DeleteAccounts(token, ids);
    }

    public async Task<Option<ErrorResult>> UpdateFieldOnAccount<T>(CancellationToken token, string accountId,
        string name, T value)
    {
        if (name is "WebRoles" or "GameRoles")
        {
            return new ErrorResult(FLAdminErrorCode.AccountFieldIsProtected,
                "WebRoles and GameRoles cannot be deleted.");
        }

        return await accountDataAccess.UpdateFieldOnAccount(accountId, name, value, token);
    }

    public async Task<Either<ErrorResult, Account>> CreateWebMaster(CancellationToken token, LoginModel loginModel)
    {
        var name = loginModel.Username.Trim();

        var accountCheck =
            await accountDataAccess.GetAccountsByFilter(account => account.Username == name, token, 1, 1);
        if (accountCheck.Count != 0)
        {
            return new ErrorResult(FLAdminErrorCode.UsernameAlreadyExists,
                $"An account with the username {loginModel.Username} already exists.");
        }

        var password = loginModel.Password.Trim();
        byte[]? salt = null;
        var hashedPass = PasswordHasher.GenerateSaltedHash(password, ref salt);

        var account = new Account
        {
            Id = config.SuperAdminName.Trim(),
            PasswordHash = hashedPass,
            Salt = salt,
            Username = name,
            WebRoles = ["SuperAdmin"]
        };

        await accountDataAccess.CreateAccounts(token, account);
        return account;
    }

    public async Task<Either<ErrorResult, Account>> GetAccountByUserName(CancellationToken token, string userName)
    {
        var accounts = await accountDataAccess.GetAccountsByFilter(account => account.Username == userName, token);
        if (accounts.Count == 0)
        {
            return new ErrorResult(FLAdminErrorCode.AccountNotFound,
                $"Account with the username {userName} does not exist.");
        }

        if (accounts.Count != 1)
        {
            logger.LogError("There exists multiple accounts with the username of {username}", userName);

            return new ErrorResult(FLAdminErrorCode.DatabaseError,
                $"An issue occured when trying to get the account with the username of {userName}");
        }

        return accounts[0];
    }

    public async Task<Either<ErrorResult, List<Account>>> GetAccountsActiveAfterDate(DateTimeOffset date,
        CancellationToken token,
        int page = 1,
        int pageSize = 100)
    {
        var accounts = await accountDataAccess.GetAccountsByFilter(x => x.LastOnline >= date, token, page, pageSize);

        if (accounts.Count == 0)
        {
            return new ErrorResult(FLAdminErrorCode.AccountNotFound,
                $"No accounts where found active after date {date}");
        }

        return accounts;
    }


    public async Task<Option<ErrorResult>> SetUpAdminAccount(string accountId,
        LoginModel login, CancellationToken token)
    {
        var accountEnum = await accountDataAccess.GetAccount(accountId, token);
        var account = accountEnum.Match<Account>(
            Left: _ => null!,
            Right: val => val
        );
        if (account == null)
        {
            return new ErrorResult(FLAdminErrorCode.AccountNotFound, $"Account with id of {accountId} does not exist.");
        }


        var username = login.Username.Trim();
        //Make sure the username is unique. 
        var found = (await accountDataAccess.GetAccountsByFilter(acc => acc.Username == username, token))
            .Count is not 0;
        if (found)
        {
            logger.LogWarning(
                "Attempt to add username {} to account {} when the username exists on another account.", username,
                accountId);
            return new ErrorResult(FLAdminErrorCode.UsernameAlreadyExists, $"{login.Username} is already in use.");
        }

        //Make sure the account does not already have a username. 
        if (account.Username != null)
        {
            logger.LogWarning("There was an attempt to add the username {} to account {} when it already has one.",
                username, account.Id);

            return new ErrorResult(FLAdminErrorCode.AccountAlreadyHasUsername, $"{accountId} already has a username.");
        }

        var password = login.Password.Trim();
        byte[]? salt = null;
        var hashedPass = PasswordHasher.GenerateSaltedHash(password, ref salt);

        account.PasswordHash = hashedPass;
        account.Salt = salt;
        account.Username = username;

        await accountDataAccess.UpdateAccount(account.ToBsonDocument(), token);
        return new Option<ErrorResult>();
    }

    public async Task<Option<ErrorResult>> ChangePassword(LoginModel login,
        string newPassword, CancellationToken token)
    {
        var username = login.Username.Trim();
        var foundAccounts = await accountDataAccess.GetAccountsByFilter(acc => acc.Username == username, token);

        if (foundAccounts.Count == 0)
        {
            return new ErrorResult(FLAdminErrorCode.AccountNotFound,
                $"Account with username {username} does not exist.");
        }


        if (foundAccounts.Count != 1)
        {
            logger.LogError("There exists multiple accounts with the username of {username}.", username);

            return new ErrorResult(FLAdminErrorCode.DatabaseError,
                $"An issue occured when trying to get the account with the username of {username}");
        }

        var account = foundAccounts[0];
        if (account.PasswordHash is null || account.Salt is null)
        {
            return new ErrorResult(FLAdminErrorCode.AccountFieldDoesNotExist,
                $"{account.Id} does not have a password set.");
        }


        //Make sure the old password is correct before changing it.
        var password = login.Password.Trim();
        var verified = PasswordHasher.VerifyPassword(password, account.PasswordHash, account.Salt);
        if (!verified)
        {
            return new ErrorResult(FLAdminErrorCode.IncorrectPassword, "Previous password is incorrect.");
        }

        byte[]? salt = null;
        var hashedPass = PasswordHasher.GenerateSaltedHash(password.Trim(), ref salt);
        account.PasswordHash = hashedPass;
        account.Salt = salt;
        return await accountDataAccess.UpdateAccount(account.ToBsonDocument(), token);
    }

    public async Task<Option<ErrorResult>> BanAccounts(List<Tuple<string, TimeSpan?>> bans, CancellationToken token)
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

        return Option<ErrorResult>.None;
    }

    public async Task<Option<ErrorResult>> UnBanAccounts(string[] ids, CancellationToken token)
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

        return Option<ErrorResult>.None;
    }

    public async Task<Option<ErrorResult>> RemoveRolesFromAccount(string id, List<Role> roles, CancellationToken token)
    {
        if (roles.Contains(Role.SuperAdmin))
        {
            return new ErrorResult(FLAdminErrorCode.SuperAdminRoleIsProtected,
                "The role of super admin cannot be removed.");
        }

        var set = roles.Select(x => x.ToString()).ToHashSet();
        var accountEnum = await accountDataAccess.GetAccount(id, token);
        var acc = accountEnum.Match<Account>(
            Left: _ => null!,
            Right: val => val
        );
        if (acc == null)
        {
            return new ErrorResult(FLAdminErrorCode.AccountNotFound, $"Account with id of {id} does not exist.");
        }


        acc.WebRoles.ExceptWith(set);
        return await accountDataAccess.UpdateFieldOnAccount(id, "webRoles", acc.WebRoles, token);
    }


    //TODO: Convert this to the proper pattern matching syntax.
    public async Task<Either<ErrorResult, bool>> IsWebMasterSetup(CancellationToken token)
    {
        var username = config.SuperAdminName.Trim();
        var webMaster = await GetAccountByUserName(token, username);

        if (!webMaster.IsLeft)
        {
            return true;
        }

        var left = webMaster.GetLeft();
        if (left.HasErrorCode(FLAdminErrorCode.AccountNotFound))
        {
            return false;
        }

        return left;
    }

    public async Task<Option<ErrorResult>> AddRolesToAccount(string id, List<Role> roles, CancellationToken token)
    {
        if (roles.Contains(Role.SuperAdmin))
        {
            return new ErrorResult(FLAdminErrorCode.SuperAdminRoleIsProtected,
                "The role of super admin cannot be added.");
        }

        var set = roles.Select(x => x.ToString()).ToHashSet();
        var accountEnum = await accountDataAccess.GetAccount(id, token);
        var acc = accountEnum.Match<Account>(
            Left: _ => null!,
            Right: val => val
        );
        if (acc == null)
        {
            return new ErrorResult(FLAdminErrorCode.AccountNotFound, $"Account with id of {id} does not exist.");
        }

        acc.WebRoles.UnionWith(set);
        return await accountDataAccess.UpdateFieldOnAccount(id, "webRoles", acc.WebRoles, token);
    }
}