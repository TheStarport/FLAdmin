using System.Linq.Expressions;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace FlAdmin.Logic.DataAccess;

public class AccountDataAccess(IDatabaseAccess databaseAccess, FlAdminConfig config, ILogger<AccountDataAccess> logger)
    : IAccountDataAccess
{
    private readonly IMongoCollection<Account> _accounts =
        databaseAccess.GetCollection<Account>(config.Mongo.AccountCollectionName);

    private readonly MongoClient _client = databaseAccess.GetClient();


    public async Task<Option<AccountError>> CreateAccounts(params Account[] accounts)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            await _accounts.InsertManyAsync(accounts);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when adding accounts.");
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> UpdateAccount(BsonDocument account)
    {
        var accountId = account.GetValue("_id").AsString;
        if (accountId is null || accountId.Length is 0) return AccountError.AccountIdIsNull;

        using var session = await _client.StartSessionAsync();
        try
        {
            var newAccount = BsonSerializer.Deserialize<Account>(account);
            var filter = Builders<Account>.Filter.Eq(a => a.Id, accountId);
            var updateDoc = Builders<Account>.Update.Set(acc => acc, newAccount);

            await _accounts.UpdateOneAsync(filter, updateDoc);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when updating account {}", accountId);
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
            logger.LogError(ex, "Mongo exception thrown when attempting to delete accounts");
            return AccountError.DatabaseError;
        }
    }

    public async Task<Either<AccountError, Account>> GetAccount(string accountId)
    {
        try
        {
            var account = (await _accounts.FindAsync(account => account.Id == accountId)).FirstOrDefault();
            if (account is null)
            {
                logger.LogWarning("Account {accountId} not found", accountId);
                return AccountError.AccountNotFound;
            }

            return account;
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Mongo exception thrown when attempting to get account of id {accountId}", accountId);
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> UpdateFieldOnAccount<T>(string accountId, string fieldName, T value)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var bsonElement = new BsonElement(fieldName, BsonValue.Create(value));
            var account = (await _accounts.FindAsync(acc => acc.Id == accountId)).FirstOrDefault().ToBsonDocument();
            if (account is null) return AccountError.AccountNotFound;

            var pair = account.Elements.FirstOrDefault(field => field.Name == bsonElement.Name);
            if (pair.Value.GetType != bsonElement.Value.GetType) return AccountError.ElementTypeMismatch;

            account.SetElement(bsonElement);
            var filter = Builders<Account>.Filter.Eq(a => a.Id, accountId);
            await _accounts.UpdateOneAsync(filter, account);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database when updating account {accountId} on field {value}",
                accountId,
                value);
            return AccountError.DatabaseError;
        }
    }

    public async Task<List<Account>> GetAccountsByFilter(Expression<Func<Account, bool>> filter, int page = 1,
        int pageSize = 100)
    {
        try
        {
            var foundAccounts = (await _accounts.FindAsync(filter)).ToList();
            return foundAccounts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database error when getting accounts by specified filter of {}",
                filter);
            return [];
        }
    }

    public async Task<Option<AccountError>> ReplaceAccount(Account account)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var filter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
            await _accounts.ReplaceOneAsync(filter, account);
            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered mongo database error when attempting to replace account of Id {} ",
                account.Id);
            return AccountError.DatabaseError;
        }
    }
}