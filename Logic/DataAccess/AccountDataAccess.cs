using System.Linq.Expressions;
using FlAdmin.Common.Configs;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models;
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

    public async Task<Option<ErrorResult>> CreateAccounts(CancellationToken token, params Account[] accounts)
    {
        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            await _accounts.InsertManyAsync(accounts, null, token);
            return new Option<ErrorResult>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database issue when adding accounts.");
            var errResult = new ErrorResult();

            if (ex is MongoBulkWriteException wx)
                if (wx.WriteErrors.Count is not 0)
                {
                    var writeError = wx.WriteErrors[0];

                    if (writeError.Code is 11000)
                    {
                        logger.LogWarning(ex, "Attempt to add accounts with ids that already exist on the database.");


                        errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountIdAlreadyExists,
                            wx.Message));
                        return errResult;
                    }
                }

            logger.LogError(ex, "Encountered a mongo database issue when adding accounts.");
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.DatabaseError, ex.Message));

            return errResult;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);

            var errResult = new ErrorResult();

            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.RequestCancelled, ex.Message));

            return errResult;
        }
    }

    public async Task<Option<ErrorResult>> UpdateAccount(BsonDocument account, CancellationToken token)
    {
        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            var errResult = new ErrorResult();

            var accountId = account.GetValue("_id").AsString;
            if (accountId is null || accountId.Length is 0)
            {
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountIdIsNull,
                    "accountID of the provided account is null."));
                return errResult;
            }

            var filter = Builders<Account>.Filter.Eq(a => a.Id, accountId);

            var updateDoc = new BsonDocument
            {
                { "$set", account }
            };

            var result = await _accounts.UpdateOneAsync(filter, updateDoc, cancellationToken: token);

            if (result.ModifiedCount is 0)
            {
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountNotFound,
                    "Account id of the provided account does not exist on the database."));
                return errResult;
            }

            return new Option<ErrorResult>();
        }
        catch (MongoException ex)
        {
            var errResult = new ErrorResult();
            logger.LogError(ex, "Encountered a mongo database issue when updating account {}",
                account.GetValue("_id").AsString);
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.DatabaseError, ex.Message));
            return errResult;
        }
        catch (KeyNotFoundException ex)
        {
            var errResult = new ErrorResult();
            logger.LogError(ex, "Attempt to update character with Bson document that does not have an ObjectId");
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountIdIsNull, ex.Message));
            return errResult;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            var errResult = new ErrorResult();
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.RequestCancelled, ex.Message));
            return errResult;
        }
    }

    public async Task<Option<ErrorResult>> DeleteAccounts(CancellationToken token, params string[] ids)
    {
        using var session = await _client.StartSessionAsync(cancellationToken: token);

        try
        {
            var errResult = new ErrorResult();
            var result = await _accounts.DeleteManyAsync(account => ids.Contains(account.Id), cancellationToken: token);

            if (result.DeletedCount is 0)
            {
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountNotFound,
                    "Account with this id is not found."));
            }


            return new Option<ErrorResult>();
        }
        catch (MongoException ex)
        {
            var errResult = new ErrorResult();
            logger.LogError(ex, "Mongo exception thrown when attempting to delete accounts");
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountIdIsNull, ex.Message));
            return errResult;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            var errResult = new ErrorResult();
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.RequestCancelled, ex.Message));
            return errResult;
        }
    }

    public async Task<Either<ErrorResult, Account>> GetAccount(string accountId, CancellationToken token)
    {
        try
        {
            var errResult = new ErrorResult();
            var account = (await _accounts.FindAsync(account => account.Id == accountId, cancellationToken: token))
                .FirstOrDefault(cancellationToken: token);
            if (account is null)
            {
                logger.LogWarning("Account {accountId} not found", accountId);

                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountNotFound, "Account not found."));
                return errResult;
            }

            return account;
        }
        catch (MongoException ex)
        {
            var errResult = new ErrorResult();
            logger.LogError(ex, "Mongo exception thrown when attempting to get account of id {accountId}", accountId);
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountIdIsNull, ex.Message));
            return errResult;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            var errResult = new ErrorResult();
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.RequestCancelled, ex.Message));
            return errResult;
        }
    }

    //TODO: Guarantee Type safety for Lists, currently any arbitrary list can be updated to a list of another type.
    public async Task<Option<ErrorResult>> UpdateFieldOnAccount<T>(string accountId, string fieldName, T value,
        CancellationToken token)
    {
        var errResult = new ErrorResult();

        switch (fieldName)
        {
            case "_id":

                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountFieldIsProtected,
                    "You cannot change the Id of an account."));
                return errResult;
            //TODO: May not work correctly
            case "username" when value is "SuperAdmin":
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountIsProtected,
                    "You cannot edit the superadmin username."));
                return errResult;
        }

        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            BsonElement newValuePair;
            if (typeof(T) == typeof(int))
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt64());
            else if (typeof(T) == typeof(float))
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToDouble());
            else
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value));

            var account = (await _accounts.FindAsync(acc => acc.Id == accountId, cancellationToken: token))
                .FirstOrDefault(cancellationToken: token).ToBsonDocument();
            if (account is null)
            {
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountNotFound,
                    $"Account Id of {accountId} not found."));
                return errResult;
            }

            if (account[fieldName] is null)
            {
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountFieldDoesNotExist,
                    $"Field {fieldName} not found."));
                return errResult;
            }

            var oldValuePair = account.Elements.FirstOrDefault(field => field.Name == newValuePair.Name);


            if (oldValuePair.Value.GetType() != newValuePair.Value.GetType())
            {
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountElementTypeMismatch,
                    "The type provided and the type in the database do not match."));
                return errResult;
            }

            account[newValuePair.Name] = newValuePair.Value;


            var accObj = BsonSerializer.Deserialize<Account>(account);
            var result = await _accounts.ReplaceOneAsync(acc => acc.Id == accountId, accObj, cancellationToken: token);

            if (result.ModifiedCount is 0)
            {
                logger.LogError("{accountId} failed to update for field {fieldName} to value {value}", accountId,
                    fieldName, value);
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.DatabaseError,
                    $"{accountId} failed to update for field {fieldName} to value {value}"));
                return errResult;
            }

            return new Option<ErrorResult>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database when updating account {accountId} on field {value}",
                accountId,
                value);
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.DatabaseError, ex.Message));
            return errResult;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Attempting to edit nonexistent field {fieldName} on account {accountId}", fieldName,
                accountId);
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountFieldDoesNotExist, ex.Message));
            return errResult;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.RequestCancelled, ex.Message));
            return errResult;
        }
    }

    public async Task<Option<ErrorResult>> CreateNewFieldOnAccount<T>(string accountId, string fieldName, T value,
        CancellationToken token)
    {
        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            var errResult = new ErrorResult();
            BsonElement keyValuePair;
            if (typeof(T) == typeof(int))
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt64());
            else if (typeof(T) == typeof(float))
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToDouble());
            else
                keyValuePair = new BsonElement(fieldName, BsonValue.Create(value));

            var account = (await _accounts.FindAsync(acc => acc.Id == accountId, cancellationToken: token))
                .FirstOrDefault(cancellationToken: token).ToBsonDocument();
            if (account is null)
            {
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountNotFound,
                    "Account with that Id doesn't exist."));
                return errResult;
            }

            account.TryGetValue(fieldName, out var val);
            if (val is not null)
            {
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountFieldAlreadyExists,
                    $"{fieldName} already exists. on {accountId}"));
                return errResult;
            }


            account.Add(keyValuePair);

            var accObj = BsonSerializer.Deserialize<Account>(account);
            var result = await _accounts.ReplaceOneAsync(acc => acc.Id == accountId, accObj);
            if (result.ModifiedCount is 0)
            {
                logger.LogError("{accountId} failed to add new field of name {fieldName} with value of {value}",
                    accountId,
                    fieldName, value);
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.DatabaseError,
                    $"{accountId} failed to add new field of name {fieldName} with value of {value}"));
                return errResult;
            }

            return new Option<ErrorResult>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database when adding new field {fieldname} on account {accountId}",
                value,
                accountId
            );
            var errResult = new ErrorResult();
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.DatabaseError, ex.Message));
            return errResult;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            var errResult = new ErrorResult();
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.RequestCancelled, ex.Message));
            return errResult;
        }
    }

    public async Task<Option<ErrorResult>> RemoveFieldOnAccount(string accountId, string fieldName,
        CancellationToken token)
    {
        var errResult = new ErrorResult();
        switch (fieldName)
        {
            case "_id":
                
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountFieldIsProtected,
                    "You cannot remove the account Id of an account."));
                return errResult;
        }

        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            var account = (await _accounts.FindAsync(acc => acc.Id == accountId, cancellationToken: token))
                .FirstOrDefault(cancellationToken: token).ToBsonDocument();
            if (account is null)
            {
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountNotFound, $"{accountId} not found."));
                return errResult;
            }


            if (account[fieldName] is null)
            {
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountFieldDoesNotExist, $"{fieldName} not found."));
                return errResult;
            }
            
            account.Remove(fieldName);
            var accObj = BsonSerializer.Deserialize<Account>(account);
            var result = await _accounts.ReplaceOneAsync(acc => acc.Id == accountId, accObj, cancellationToken: token);
            
            if (result.ModifiedCount is 0)
            {
                logger.LogError("{accountId} failed to remove field with name of {fieldName}",
                    accountId,
                    fieldName);
                errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.DatabaseError,
                    $"{accountId} failed to remove field of name {fieldName}"));
                return errResult;
            }
            
            return new Option<ErrorResult>();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex,
                "Encountered a mongo database when attempting to remove field {fieldName} from account {accountId}",
                fieldName, accountId);
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.DatabaseError, ex.Message));
            return errResult;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Attempting to remove a nonexistent field {fieldName} on account {accountId}",
                fieldName,
                accountId);
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountFieldDoesNotExist, ex.Message));
            return errResult;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.RequestCancelled, ex.Message));
            return errResult;
        }
    }

    public async Task<List<Account>> GetAccountsByFilter(Expression<Func<Account, bool>> filter,
        CancellationToken token, int page = 1,
        int pageSize = 100)
    {
        try
        {
            var foundAccounts = (await _accounts.FindAsync(filter, cancellationToken: token)).ToList();
            return foundAccounts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
        catch (MongoException ex)
        {
            logger.LogError(ex, "Encountered a mongo database error when getting accounts by specified filter of {}",
                filter);
            return [];
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            return [];
        }
    }

    public async Task<Option<ErrorResult>> ReplaceAccount(Account account, CancellationToken token)
    {
        using var session = await _client.StartSessionAsync(cancellationToken: token);
        try
        {
            var filter = Builders<Account>.Filter.Eq(a => a.Id, account.Id);
            await _accounts.ReplaceOneAsync(filter, account, cancellationToken: token);
            return new Option<ErrorResult>();
        }
        catch (MongoException ex)
        {
            var errResult = new ErrorResult();
            logger.LogError(ex, "Encountered mongo database error when attempting to replace account of Id {} ",
                account.Id);
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.AccountFieldDoesNotExist, ex.Message));
            return errResult;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Request was cancelled{}", ex.Message);
            var errResult = new ErrorResult();
            errResult.Errors.Add(new FLAdminError(FLAdminErrorCode.RequestCancelled, ex.Message));
            return errResult;
        }
    }
}