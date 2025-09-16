using System.Linq.Expressions;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.DataAccess;

public interface IAccountDataAccess
{
    Task<Option<FLAdminError>> CreateAccounts(CancellationToken token, params Account[] accounts);
    Task<Option<FLAdminError>> UpdateAccount(BsonDocument account, CancellationToken token);
    Task<Option<FLAdminError>> DeleteAccounts(CancellationToken token, params string[] ids);
    Task<Either<FLAdminError, Account>> GetAccount(string accountId, CancellationToken token);
    Task<Option<FLAdminError>> UpdateFieldOnAccount<T>(string accountId, string fieldName, T value,
        CancellationToken token);
    Task<Option<FLAdminError>> CreateNewFieldOnAccount<T>(string accountId, string fieldName, T value,
        CancellationToken token);
    Task<Option<FLAdminError>> RemoveFieldOnAccount(string accountId, string fieldName, CancellationToken token);

    Task<List<Account>> GetAccountsByFilter(Expression<Func<Account, bool>> filter, CancellationToken token,
        int page = 1, int pageSize = 100);
    Task<Option<FLAdminError>> ReplaceAccount(Account account, CancellationToken token);
}