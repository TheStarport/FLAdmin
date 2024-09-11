using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using MongoDB.Bson;

namespace FlAdmin.Common.Services;

public interface IAccountService
{
    Task<List<Account>> GetAllAccounts();

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
    Task<List<Account>> GetAccountsActiveAfterDate(DateTimeOffset date);
    Task AddRolesToAccount(string id, List<Role> roles);
    Task SetUpAdminAccount(string accountId, LoginModel login);

    Task ChangePassword(LoginModel login, string oldPassword);
    
    Task BanAccount(string id, TimeSpan? duration);

    Task UnBanAccount(string id);
    
    Task RemoveRolesFromAccount(string id, List<Role> roles);
    

}