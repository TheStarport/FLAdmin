using System.Linq.Expressions;
using FlAdmin.Common.Models;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.DataAccess;

public interface IAccountDataAccess
{
    /// <summary>
    /// Creates accounts on the database
    /// </summary>
    /// <param name="token"></param>
    /// <param name="accounts">list of accounts to be added.</param>
    /// <returns>Error object if failed.</returns>
    Task<Option<ErrorResult>> CreateAccounts(CancellationToken token, params Account[] accounts);
    Task<Option<ErrorResult>> UpdateAccount(BsonDocument account, CancellationToken token);
    Task<Option<ErrorResult>> DeleteAccounts(CancellationToken token, params string[] ids);
    Task<Either<ErrorResult, Account>> GetAccount(string accountId, CancellationToken token);
    Task<Option<ErrorResult>> UpdateFieldOnAccount<T>(string accountId, string fieldName, T value,
        CancellationToken token);
    Task<Option<ErrorResult>> CreateNewFieldOnAccount<T>(string accountId, string fieldName, T value,
        CancellationToken token);
    Task<Option<ErrorResult>> RemoveFieldOnAccount(string accountId, string fieldName, CancellationToken token);

    Task<List<Account>> GetAccountsByFilter(Expression<Func<Account, bool>> filter, CancellationToken token,
        int page = 1, int pageSize = 100);
    Task<Option<ErrorResult>> ReplaceAccount(Account account, CancellationToken token);
}