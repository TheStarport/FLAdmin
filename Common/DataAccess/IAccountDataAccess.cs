using System.Linq.Expressions;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;
using MongoDB.Bson;

namespace FlAdmin.Common.DataAccess;

public interface IAccountDataAccess
{
    Task<Option<AccountError>> CreateAccounts(params Account[] accounts);
    Task<Option<AccountError>> UpdateAccount(BsonDocument account);
    Task<Option<AccountError>> DeleteAccounts(params string[] ids);
    Task<Either<AccountError, Account>> GetAccount(string accountId);
    Task<Option<AccountError>> UpdateFieldOnAccount<T>(string accountId, string fieldName, T value);
    Task<List<Account>> GetAccountsByFilter(Expression<Func<Account, bool>> filter, int page = 1, int pageSize = 100);
    Task<Option<AccountError>> ReplaceAccount(Account account);
}