using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using Bogus;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using FlAdmin.Service.Extensions;
using MongoDB.Bson;
using System.Collections.Generic;
using LanguageExt;


namespace FlAdmin.Tests.DataAccessMocks;

public class AccountDataAccessMock : IAccountDataAccess, IDisposable
{
    List<Account> _accounts;

    public AccountDataAccessMock()
    {
        _accounts = HelperFunctions.GenerateRandomAccounts(HelperFunctions.GenerateRandomCharacters());
    }


    public Task<Option<FLAdminError>> CreateAccounts(params Account[] accounts)
    {
        foreach (var account in accounts)
        {
            if (_accounts.Any(acc => acc.Id == account.Id))
            {
                return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountIdAlreadyExists);
            }
        }

        return Task.FromResult(new Option<FLAdminError>());
    }

    public Task<Option<FLAdminError>> UpdateAccount(BsonDocument account)
    {
        var accountId = account.GetValue("_id").AsString;
        if (accountId is null || accountId.Length is 0)
            return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountIdIsNull);

        if (_accounts.All(x => x.Id != accountId))
        {
            return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountNotFound);
        }

        return Task.FromResult(new Option<FLAdminError>());
    }

    public Task<Option<FLAdminError>> DeleteAccounts(params string[] ids)
    {
        if (ids.ToList().Any(_ => ids.Contains("SuperAdmin")))
        {
            return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountIsProtected);
        }

        return ids.Any(id => _accounts.Any(x => x.Id == id))
            ? Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountNotFound)
            : Task.FromResult(new Option<FLAdminError>());
    }

    public Task<Either<FLAdminError, Account>> GetAccount(string accountId)
    {
        var account = _accounts.FirstOrDefault(x => x.Id == accountId);
        return account is null
            ? Task.FromResult<Either<FLAdminError, Account>>(FLAdminError.AccountNotFound)
            : Task.FromResult<Either<FLAdminError, Account>>(account);
    }

    public Task<Option<FLAdminError>> UpdateFieldOnAccount<T>(string accountId, string fieldName, T value)
    {
        switch (fieldName)
        {
            case "_id":
                return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountFieldIsProtected);
            //TODO: May not work correctly
            case "username" when value is "SuperAdmin":
                return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountIsProtected);
        }

        var account = _accounts.FirstOrDefault(x => x.Id == accountId);
        if (account is null)
        {
            return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountNotFound);
        }


        var doc = account.ToBsonDocument();
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

            var oldPair = doc.Elements.First(el => el.Name == fieldName);

            if (oldPair.Value.GetType() != newValuePair.Value.GetType())
            {
                return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountElementTypeMismatch);
            }
        }
        catch (KeyNotFoundException ex)
        {
            return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountFieldDoesNotExist);
        }

        return Task.FromResult(new Option<FLAdminError>());
    }

    public Task<Option<FLAdminError>> CreateNewFieldOnAccount<T>(string accountId, string fieldName, T value)
    {
        switch (fieldName)
        {
            case "_id":
                return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountFieldIsProtected);
            //TODO: May not work correctly
            case "username" when value is "SuperAdmin":
                return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountIsProtected);
        }

        var account = _accounts.FirstOrDefault(a => a.Id == accountId);
        if (account is null)
        {
            return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountNotFound);
        }

        var doc = account.ToBsonDocument();
        doc.TryGetValue(fieldName, out var element);
        return element is not null
            ? Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountFieldAlreadyExists)
            : Task.FromResult(new Option<FLAdminError>());
    }

    public Task<Option<FLAdminError>> RemoveFieldOnAccount(string accountId, string fieldName)
    {
        switch (fieldName)
        {
            case "_id":
                return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountFieldIsProtected);
        }

        var account = _accounts.FirstOrDefault(a => a.Id == accountId);
        if (account is null)
        {
            return Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountNotFound);
        }

        var doc = account.ToBsonDocument();
        doc.TryGetValue(fieldName, out var element);
        return element is null
            ? Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountFieldDoesNotExist)
            : Task.FromResult(new Option<FLAdminError>());
    }


    public Task<List<Account>> GetAccountsByFilter(Expression<Func<Account, bool>> filter, int page = 1,
        int pageSize = 100)
    {
        var func = filter.Compile();
        var accounts = _accounts.Filter(func).Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(accounts);
    }

    public Task<Option<FLAdminError>> ReplaceAccount(Account account)
    {
        return _accounts.All(x => x.Id != account.Id)
            ? Task.FromResult<Option<FLAdminError>>(FLAdminError.AccountNotFound)
            : Task.FromResult(new Option<FLAdminError>());
    }


    public void Dispose()
    {
        _accounts.Clear();
    }
}