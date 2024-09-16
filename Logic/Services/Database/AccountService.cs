using System.Diagnostics;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Common.Services;
using FlAdmin.Logic.Services.Auth;
using LanguageExt;
using MongoDB.Bson;
using MongoDB.Driver;
using Optional = LanguageExt.Optional;

namespace FlAdmin.Logic.Services.Database;

public class AccountService(IDatabaseAccess databaseAccess, FlAdminConfig config, ILogger<AccountService> logger)
    : IAccountService
{
    private readonly IMongoCollection<Account>
        _accounts = databaseAccess.GetDatabase().GetCollection<Account>(config.Mongo.AccountCollectionName);

    private readonly MongoClient _client = databaseAccess.GetClient();

    public async Task<List<Account>> GetAllAccounts()
    {
        var foundDoc = await _accounts.FindAsync(account => true);
        return foundDoc.ToList();
    }

    public List<Account> QueryAccounts(IQueryable<Account> query)
    {
        throw new NotImplementedException();
    }

    public async Task<Either<AccountError, Account>> GetAccountById(string id)
    {
        var account = (await _accounts.FindAsync(account => account.Id == id)).FirstOrDefault();
        if (account is null)
            return AccountError.AccountNotFound;
        return account;
    }

    public async Task<Option<AccountError>> AddAccounts(params Account[] accounts)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            await _accounts.InsertManyAsync(accounts);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError("Encountered a mongo database issue when adding accounts, info:{}", ex.ToString());
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> UpdateAccount(Account account)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var filter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
            var update = Builders<Account>.Update.Set(acc => acc, account);
            await _accounts.UpdateOneAsync(filter, update);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError("Encountered a mongo database issue when updating account {}, info:{}", account.Id,
                ex.ToString());
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> DeleteAccounts(params string[] ids)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            await _accounts.DeleteManyAsync(account => ids.Contains(account.Id));
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError("Encountered a mongo database issue when attempting to delete account(s) {}, info:{}", ids,
                ex);
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> UpdateFieldOnAccount(BsonElement bsonElement, string accountId)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var account = (await _accounts.FindAsync(acc => acc.Id == accountId)).FirstOrDefault().ToBsonDocument();
            if (account is null)
            {
                return AccountError.AccountNotFound;
            }

            var pair = account.Elements.FirstOrDefault(field => field.Name == bsonElement.Name);
            if (pair.Value.GetType != bsonElement.Value.GetType)
            {
                return AccountError.ElementTypeMismatch;
            }

            account.SetElement(bsonElement);
            var filter = Builders<Account>.Filter.Eq(a => a.Id, accountId);
            await _accounts.UpdateOneAsync(filter, account);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError("Encountered a mongo database when updating account {} on field {}, info:{}", accountId,
                bsonElement.Name, ex.ToString());
            return AccountError.DatabaseError;
        }
    }

    public List<Account> GetOnlineAccounts()
    {
        logger.LogError("Attempted usage of unimplemented function.");
        throw new NotImplementedException();
    }

    public async Task<Option<AccountError>> CreateWebMaster(LoginModel loginModel)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var name = loginModel.Username.Trim();
            if (await _accounts!.CountDocumentsAsync(Builders<Account>.Filter.Eq("username", name)) is not 0)
            {
                return AccountError.UsernameAlreadyExists;
            }

            var password = loginModel.Password.Trim();
            byte[]? salt = null;
            var hashedPass = PasswordHasher.GenerateSaltedHash(password, ref salt);
            var account = new Account
            {
                Id = ObjectId.GenerateNewId().ToString(),
                PasswordHash = hashedPass,
                Salt = salt,
                Username = name,
                WebRoles = []
            };
            await _accounts.InsertOneAsync(account);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogCritical(ex, "Failed to create webmaster on database.");
            return AccountError.DatabaseError;
        }
    }

    public async Task<Either<AccountError, Account>> GetAccountByUserName(string userName)
    {
        try
        {
            var account = (await _accounts.FindAsync(account => account.Username == userName)).FirstOrDefault();

            if (account is null)
            {
                return AccountError.AccountNotFound;
            }

            //TODO: Log warning of account not having a password with a username existing.
            if (account.PasswordHash is null || account.Salt is null)
            {
                logger.LogWarning("Account {} has a username {} with a null password", account.Id, userName);
            }

            return account;
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when fetching account with username of {}",
                userName);
            return AccountError.DatabaseError;
        }
    }

    public async Task<Either<AccountError, List<Account>>> GetAccountsActiveAfterDate(DateTimeOffset date)
    {
        try
        {
            return (await _accounts.FindAsync(x => x.LastOnline >= date)).ToList();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when fetching accounts after active date {}",
                date.ToString());
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> AddRolesToAccount(string id, List<Role> roles)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var filter = Builders<Account>.Filter.Eq(a => a.Id, id);
            var update =
                Builders<Account>.Update.AddToSetEach(a => a.WebRoles, roles.Select(x => x.ToString()).ToList());
            await _accounts.FindOneAndUpdateAsync(filter, update);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when fetching accounts after active date {}.",
                roles);
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> SetUpAdminAccount(string accountId, LoginModel login)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var account = (await _accounts.FindAsync(acc => acc.Id == accountId)).FirstOrDefault();
            if (account is null)
            {
                return AccountError.AccountNotFound;
            }

            var username = login.Username.Trim();
            //Make sure the username is unique. 
            var found = (await _accounts.FindAsync(acc => acc.Username == username)).FirstOrDefault() is not null;
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

            var update = Builders<Account>.Update.Set(a => a.PasswordHash, hashedPass).Set(a => a.Salt, salt)
                .Set(a => a.Username, account.Username);
            var filter = Builders<Account>.Filter.Eq(a => a.Id, accountId);
            await _accounts.FindOneAndUpdateAsync(filter, update);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when setting up admin account for accountId {}.",
                accountId);
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> ChangePassword(LoginModel login, string newPassword)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var username = login.Username.Trim();
            var account = (await _accounts.FindAsync(acc => acc.Username == username)).FirstOrDefault();
            if (account is null)
            {
                return AccountError.AccountNotFound;
            }

            if (account.PasswordHash is null || account.Salt is null)
            {
                return AccountError.FieldDoesNotExist;
            }

            //Make sure the old password is correct before changing it.
            var password = login.Password.Trim();
            var verified = PasswordHasher.VerifyPassword(password, account.PasswordHash, account.Salt);
            if (!verified)
            {
                return AccountError.IncorrectPassword;
            }

            byte[]? salt = null;
            var hashedPass = PasswordHasher.GenerateSaltedHash(password.Trim(), ref salt);
            var update = Builders<Account>.Update.Set(a => a.PasswordHash, hashedPass).Set(a => a.Salt, salt);
            var filter = Builders<Account>.Filter.Eq(a => a.Username, username);
            await _accounts.FindOneAndUpdateAsync(filter, update);

            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when attempting to change password for username {}",
                login.Username);
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> BanAccount(string id, TimeSpan? duration)
    {
        using var session = await _client.StartSessionAsync();

        try
        {
            duration ??= TimeSpan.FromDays(109500);
            var filter = Builders<Account>.Filter.Eq(a => a.Id, id);
            var update = Builders<Account>.Update.Set(a => a.ScheduledUnbanDate, DateTimeOffset.Now + duration);
            await _accounts.FindOneAndUpdateAsync(filter, update);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when attempting to ban account with id of {}",
                id);
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> UnBanAccount(string id)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var filter = Builders<Account>.Filter.Eq(a => a.Id, id);
            var update = Builders<Account>.Update.Set(a => a.ScheduledUnbanDate, null);
            await _accounts.FindOneAndUpdateAsync(filter, update);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when attempting to unban account with id of {}",
                id);
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> RemoveRolesFromAccount(string id, List<Role> roles)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var account = (await _accounts.FindAsync(account => account.Id == id)).FirstOrDefault();
            if (account is null)
            {
                return AccountError.AccountNotFound;
            }

            var roleStrList = roles.Select(x => x.ToString()).ToList();
            account.WebRoles.RemoveAll(r => roleStrList.Exists(str => str == r));
            var filter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
            await _accounts.ReplaceOneAsync(filter, account);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex,
                "Encountered a mongo database issue when attempting to remove roles from account with id of {}",
                id);
            return AccountError.DatabaseError;
        }
    }
}