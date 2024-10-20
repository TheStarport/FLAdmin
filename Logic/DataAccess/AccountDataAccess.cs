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

            if (ex is MongoBulkWriteException wx)
            {
                if (wx.WriteErrors.Count is not 0)
                {
                    var writeError = wx.WriteErrors[0];

                    if (writeError.Code is 11000)
                    {
                        return AccountError.AccountIdAlreadyExists;
                    }
                }
            }

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

            var result = await _accounts.UpdateOneAsync(filter, updateDoc);

            if (result.ModifiedCount is 0)
            {
                return AccountError.AccountNotFound;
            }

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

        if (ids.Contains("SuperAdmin"))
        {
            return AccountError.AccountIsProtected;
        }

        try
        {
            var result = await _accounts.DeleteManyAsync(account => ids.Contains(account.Id));

            if (result.DeletedCount is 0)
            {
                return AccountError.AccountNotFound;
            }

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
        switch (fieldName)
        {
            case "_id":
                return AccountError.FieldIsProtected;
            //TODO: May not work correctly
            case "username" when value is "SuperAdmin":
                return AccountError.AccountIsProtected;
        }


        using var session = await _client.StartSessionAsync();
        try
        {
            BsonElement newValuePair;
            if (typeof(T) == typeof(int))
            {
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt64());
            }
            else if (typeof(T) == typeof(float))
            {
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToDouble());
            }
            else
            {
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value));
            }

            var account = (await _accounts.FindAsync(acc => acc.Id == accountId)).FirstOrDefault().ToBsonDocument();
            if (account is null)
            {
                return AccountError.AccountNotFound;
            }

            if (account[fieldName] is null)
            {
                return AccountError.FieldDoesNotExist;
            }

            var oldValuePair = account.Elements.FirstOrDefault(field => field.Name == newValuePair.Name);

            if (oldValuePair.Value.GetType() != newValuePair.Value.GetType())
            {
                return AccountError.ElementTypeMismatch;
            }

            account[newValuePair.Name] = newValuePair.Value;


            var accObj = BsonSerializer.Deserialize<Account>(account);
            var result = await _accounts.ReplaceOneAsync(acc => acc.Id == accountId, accObj);

            if (result.ModifiedCount is 0)
            {
                logger.LogError("{accountId} failed to update for field {fieldName} to value {value}", accountId,
                    fieldName, value);
                return AccountError.DatabaseError;
            }

            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database when updating account {accountId} on field {value}",
                accountId,
                value);
            return AccountError.DatabaseError;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Attempting to edit nonexistent field {fieldName} on account {accountId}", fieldName,
                accountId);
            return AccountError.FieldDoesNotExist;
        }
    }

    public async Task<Option<AccountError>> CreateNewFieldOnAccount<T>(string accountId, string fieldName, T value)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            BsonElement keyValuePair;
            if (typeof(T) == typeof(int))
            {
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt64());
            }
            else if (typeof(T) == typeof(float))
            {
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToDouble());
            }
            else
            {
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value));
            }

            var account = (await _accounts.FindAsync(acc => acc.Id == accountId)).FirstOrDefault().ToBsonDocument();
            if (account is null)
            {
                return AccountError.AccountNotFound;
            }

            account.TryGetValue(fieldName, out var val);
            if (val is not null)
            {
                return AccountError.FieldAlreadyExists;
            }

            account.Add(keyValuePair);

            var accObj = BsonSerializer.Deserialize<Account>(account);
            var result = await _accounts.ReplaceOneAsync(acc => acc.Id == accountId, accObj);
            if (result.ModifiedCount is 0)
            {
                logger.LogError("{accountId} failed to add new field of name {fieldName} with value of {value}",
                    accountId,
                    fieldName, value);
                return AccountError.DatabaseError;
            }

            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database when adding new field {fieldname} on account {accountId}",
                value,
                accountId
            );
            return AccountError.DatabaseError;
        }
    }

    public async Task<Option<AccountError>> RemoveFieldOnAccount(string accountId, string fieldName)
    {
        switch (fieldName)
        {
            case "_id":
                return AccountError.FieldIsProtected;
        }


        using var session = await _client.StartSessionAsync();
        try
        {
            var account = (await _accounts.FindAsync(acc => acc.Id == accountId)).FirstOrDefault().ToBsonDocument();
            if (account is null)
            {
                return AccountError.AccountNotFound;
            }

            if (account[fieldName] is null)
            {
                return AccountError.FieldDoesNotExist;
            }

            account.Remove(fieldName);
            var accObj = BsonSerializer.Deserialize<Account>(account);
            var result = await _accounts.ReplaceOneAsync(acc => acc.Id == accountId, accObj);
            if (result.ModifiedCount is 0)
            {
                return AccountError.DatabaseError;
            }

            return new Option<AccountError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex,
                "Encountered a mongo database when attempting to remove field {fieldName} from account {accountId}",
                fieldName, accountId);
            return AccountError.DatabaseError;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Attempting to remove a nonexistent field {fieldName} on account {accountId}",
                fieldName,
                accountId);
            return AccountError.FieldDoesNotExist;
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