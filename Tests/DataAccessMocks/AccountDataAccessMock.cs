using System.Linq.Expressions;
using FlAdmin.Common.DataAccess;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Tests.DataAccessMocks;

public class AccountDataAccessMock : IAccountDataAccess, IDisposable
{
    private readonly List<Account> _accounts;

    public AccountDataAccessMock()
    {
        _accounts = HelperFunctions.GenerateRandomAccounts(HelperFunctions.GenerateRandomCharacters());
    }


    public Task<Option<FLAdminErrorCode>> CreateAccounts(CancellationToken token, params Account[] accounts)
    {
        foreach (var account in accounts)
            if (_accounts.Any(acc => acc.Id == account.Id))
                return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountIdAlreadyExists);

        return Task.FromResult(new Option<FLAdminErrorCode>());
    }

    public Task<Option<FLAdminErrorCode>> UpdateAccount(BsonDocument account, CancellationToken token)
    {
        var accountId = account.GetValue("_id").AsString;
        if (accountId is null || accountId.Length is 0)
            return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountIdIsNull);

        if (_accounts.All(x => x.Id != accountId))
            return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountNotFound);

        return Task.FromResult(new Option<FLAdminErrorCode>());
    }

    public Task<Option<FLAdminErrorCode>> DeleteAccounts(CancellationToken token, params string[] ids)
    {
        if (ids.ToList().Any(_ => ids.Contains("SuperAdmin")))
            return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountIsProtected);

        return ids.Any(id => _accounts.Any(x => x.Id == id))
            ? Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountNotFound)
            : Task.FromResult(new Option<FLAdminErrorCode>());
    }

    public Task<Either<FLAdminErrorCode, Account>> GetAccount(string accountId, CancellationToken token)
    {
        var account = _accounts.FirstOrDefault(x => x.Id == accountId);
        return account is null
            ? Task.FromResult<Either<FLAdminErrorCode, Account>>(FLAdminErrorCode.AccountNotFound)
            : Task.FromResult<Either<FLAdminErrorCode, Account>>(account);
    }

    public Task<Option<FLAdminErrorCode>> UpdateFieldOnAccount<T>(string accountId, string fieldName, T value,
        CancellationToken token)
    {
        switch (fieldName)
        {
            case "_id":
                return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountFieldIsProtected);
            //TODO: May not work correctly
            case "username" when value is "SuperAdmin":
                return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountIsProtected);
        }

        var account = _accounts.FirstOrDefault(x => x.Id == accountId);
        if (account is null) return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountNotFound);


        var doc = account.ToBsonDocument();
        try
        {
            BsonElement newValuePair;
            if (typeof(T) == typeof(int))
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToInt64());
            else if (typeof(T) == typeof(float))
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value).ToDouble());
            else
                newValuePair = new BsonElement(fieldName, BsonValue.Create(value));

            var oldPair = doc.Elements.First(el => el.Name == fieldName);

            if (oldPair.Value.GetType() != newValuePair.Value.GetType())
                return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountElementTypeMismatch);
        }
        catch (KeyNotFoundException ex)
        {
            return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountFieldDoesNotExist);
        }

        return Task.FromResult(new Option<FLAdminErrorCode>());
    }

    public Task<Option<FLAdminErrorCode>> CreateNewFieldOnAccount<T>(string accountId, string fieldName, T value,
        CancellationToken token)
    {
        switch (fieldName)
        {
            case "_id":
                return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountFieldIsProtected);
            //TODO: May not work correctly
            case "username" when value is "SuperAdmin":
                return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountIsProtected);
        }

        var account = _accounts.FirstOrDefault(a => a.Id == accountId);
        if (account is null) return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountNotFound);

        var doc = account.ToBsonDocument();
        doc.TryGetValue(fieldName, out var element);
        return element is not null
            ? Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountFieldAlreadyExists)
            : Task.FromResult(new Option<FLAdminErrorCode>());
    }

    public Task<Option<FLAdminErrorCode>> RemoveFieldOnAccount(string accountId, string fieldName, CancellationToken token)
    {
        switch (fieldName)
        {
            case "_id":
                return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountFieldIsProtected);
        }

        var account = _accounts.FirstOrDefault(a => a.Id == accountId);
        if (account is null) return Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountNotFound);

        var doc = account.ToBsonDocument();
        doc.TryGetValue(fieldName, out var element);
        return element is null
            ? Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountFieldDoesNotExist)
            : Task.FromResult(new Option<FLAdminErrorCode>());
    }


    public Task<List<Account>> GetAccountsByFilter(Expression<Func<Account, bool>> filter, CancellationToken token,
        int page = 1,
        int pageSize = 100)
    {
        var func = filter.Compile();
        var accounts = _accounts.Filter(func).Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(accounts);
    }

    public Task<Option<FLAdminErrorCode>> ReplaceAccount(Account account, CancellationToken token)
    {
        return _accounts.All(x => x.Id != account.Id)
            ? Task.FromResult<Option<FLAdminErrorCode>>(FLAdminErrorCode.AccountNotFound)
            : Task.FromResult(new Option<FLAdminErrorCode>());
    }


    public void Dispose()
    {
        _accounts.Clear();
    }
}