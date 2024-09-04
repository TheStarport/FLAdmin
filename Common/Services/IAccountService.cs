using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface IAccountService
{
    List<Account> GetAllAccounts();

    List<Account> QueryAccounts(IQueryable<Account> query);

    Task<Account?> GetAccountById(string id);

    //TODO: Return type enums for error handling and logging. 
    Task AddAccounts(params Account[] accounts);
    Task UpdateAccount(Account account);
    Task DeleteAccounts(params string[] ids);

    Task UpdateFieldOnAccount(BsonElement bsonElement, string accountId);

    List<Account> GetOnlineAccounts();
    Task<bool> CreateWebMaster(LoginModel loginModel);

    Task<Account?> GetAccountByUserName(string userName);
}