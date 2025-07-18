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

    public async Task<Option<FLAdminError>> CreateAccounts(params Account[] accounts)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            await _accounts.InsertManyAsync(accounts);
            return new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when adding accounts.");

            if (ex is MongoBulkWriteException wx)
                if (wx.WriteErrors.Count is not 0)
                {
                    var writeError = wx.WriteErrors[0];

                    if (writeError.Code is 11000)
                    {
                        logger.LogWarning(ex, "Attempt to add accounts with ids that already exist on the database.");
                        return FLAdminError.AccountIdAlreadyExists;
                    }
                }

            logger.LogError(ex, "Encountered a mongo database issue when adding accounts.");
            return FLAdminError.DatabaseError;
        }
    }

    public async Task<Option<FLAdminError>> UpdateAccount(BsonDocument account)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var accountId = account.GetValue("_id").AsString;
            if (accountId is null || accountId.Length is 0) return FLAdminError.AccountIdIsNull;

            var filter = Builders<Account>.Filter.Eq(a => a.Id, accountId);

            var updateDoc = new BsonDocument
            {
                { "$set", account }
            };

            var result = await _accounts.UpdateOneAsync(filter, updateDoc);

            if (result.ModifiedCount is 0) return FLAdminError.AccountNotFound;

            return new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when updating account {}",
                account.GetValue("_id").AsString);
            return FLAdminError.DatabaseError;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, "Attempt to update character with Bson document that does not have an ObjectId");
            return FLAdminError.AccountIdIsNull;
        }
    }

    public async Task<Option<FLAdminError>> DeleteAccounts(params string[] ids)
    {
        using var session = await _client.StartSessionAsync();
        
        try
        {
            var result = await _accounts.DeleteManyAsync(account => ids.Contains(account.Id));

            if (result.DeletedCount is 0) return FLAdminError.AccountNotFound;

            return new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Mongo exception thrown when attempting to delete accounts");
            return FLAdminError.DatabaseError;
        }
    }

    public async Task<Either<FLAdminError, Account>> GetAccount(string accountId)
    {
        try
        {
            var account = (await _accounts.FindAsync(account => account.Id == accountId)).FirstOrDefault();
            if (account is null)
            {
                logger.LogWarning("Account {accountId} not found", accountId);
                return FLAdminError.AccountNotFound;
            }

            return account;
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Mongo exception thrown when attempting to get account of id {accountId}", accountId);
            return FLAdminError.DatabaseError;
        }
    }

    //TODO: Guarantee Type safety for Lists, currently any arbitrary list can be updated to a list of another type.
    public async Task<Option<FLAdminError>> UpdateFieldOnAccount<T>(string accountId, string fieldName, T value)
    {
        switch (fieldName)
        {
            case "_id":
                return FLAdminError.AccountFieldIsProtected;
            //TODO: May not work correctly
            case "username" when value is "SuperAdmin":
                return FLAdminError.AccountIsProtected;
        }

        using var session = await _client.StartSessionAsync();
        try
        {
            BsonElement newValuePair;
            if (typeof(T) == typeof(int))
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt64());
            else if (typeof(T) == typeof(float))
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToDouble());
            else
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value));

            var account = (await _accounts.FindAsync(acc => acc.Id == accountId)).FirstOrDefault().ToBsonDocument();
            if (account is null) return FLAdminError.AccountNotFound;

            if (account[fieldName] is null) return FLAdminError.AccountFieldDoesNotExist;


            var oldValuePair = account.Elements.FirstOrDefault(field => field.Name == newValuePair.Name);


            if (oldValuePair.Value.GetType() != newValuePair.Value.GetType())
                return FLAdminError.AccountElementTypeMismatch;

            account[newValuePair.Name] = newValuePair.Value;


            var accObj = BsonSerializer.Deserialize<Account>(account);
            var result = await _accounts.ReplaceOneAsync(acc => acc.Id == accountId, accObj);

            if (result.ModifiedCount is 0)
            {
                logger.LogError("{accountId} failed to update for field {fieldName} to value {value}", accountId,
                    fieldName, value);
                return FLAdminError.DatabaseError;
            }

            return new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database when updating account {accountId} on field {value}",
                accountId,
                value);
            return FLAdminError.DatabaseError;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Attempting to edit nonexistent field {fieldName} on account {accountId}", fieldName,
                accountId);
            return FLAdminError.AccountFieldDoesNotExist;
        }
    }

    public async Task<Option<FLAdminError>> CreateNewFieldOnAccount<T>(string accountId, string fieldName, T value)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            BsonElement keyValuePair;
            if (typeof(T) == typeof(int))
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt64());
            else if (typeof(T) == typeof(float))
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToDouble());
            else
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value));

            var account = (await _accounts.FindAsync(acc => acc.Id == accountId)).FirstOrDefault().ToBsonDocument();
            if (account is null) return FLAdminError.AccountNotFound;

            account.TryGetValue(fieldName, out var val);
            if (val is not null) return FLAdminError.AccountFieldAlreadyExists;

            account.Add(keyValuePair);

            var accObj = BsonSerializer.Deserialize<Account>(account);
            var result = await _accounts.ReplaceOneAsync(acc => acc.Id == accountId, accObj);
            if (result.ModifiedCount is 0)
            {
                logger.LogError("{accountId} failed to add new field of name {fieldName} with value of {value}",
                    accountId,
                    fieldName, value);
                return FLAdminError.DatabaseError;
            }

            return new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database when adding new field {fieldname} on account {accountId}",
                value,
                accountId
            );
            return FLAdminError.DatabaseError;
        }
    }

    public async Task<Option<FLAdminError>> RemoveFieldOnAccount(string accountId, string fieldName)
    {
        switch (fieldName)
        {
            case "_id":
                return FLAdminError.AccountFieldIsProtected;
        }

        using var session = await _client.StartSessionAsync();
        try
        {
            var account = (await _accounts.FindAsync(acc => acc.Id == accountId)).FirstOrDefault().ToBsonDocument();
            if (account is null) return FLAdminError.AccountNotFound;

            if (account[fieldName] is null) return FLAdminError.AccountFieldDoesNotExist;

            account.Remove(fieldName);
            var accObj = BsonSerializer.Deserialize<Account>(account);
            var result = await _accounts.ReplaceOneAsync(acc => acc.Id == accountId, accObj);
            if (result.ModifiedCount is 0) return FLAdminError.DatabaseError;

            return new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex,
                "Encountered a mongo database when attempting to remove field {fieldName} from account {accountId}",
                fieldName, accountId);
            return FLAdminError.DatabaseError;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Attempting to remove a nonexistent field {fieldName} on account {accountId}",
                fieldName,
                accountId);
            return FLAdminError.AccountFieldDoesNotExist;
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

    public async Task<Option<FLAdminError>> ReplaceAccount(Account account)
    {
        using var session = await _client.StartSessionAsync();
        try
        {
            var filter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
            await _accounts.ReplaceOneAsync(filter, account);
            return new Option<FLAdminError>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered mongo database error when attempting to replace account of Id {} ",
                account.Id);
            return FLAdminError.DatabaseError;
        }
    }
}