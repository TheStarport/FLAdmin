using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;

namespace FlAdmin.Common.Services;

public interface IAccountService
{
    Task<List<Account>> GetAllAccounts();

    List<Account> QueryAccounts(IQueryable<Account> query);

    Task<Either<AccountError, Account>> GetAccountById(string id);

    Task<Option<AccountError>> CreateAccounts(params Account[] accounts);
    Task<Option<AccountError>> UpdateAccount(Account account);
    Task<Option<AccountError>> DeleteAccounts(params string[] ids);

    Task<Option<AccountError>> UpdateFieldOnAccount<T>(string accountId, string name, T value);

    List<Account> GetOnlineAccounts();
    Task<Option<AccountError>> CreateWebMaster(LoginModel loginModel);

    Task<Either<AccountError, Account>> GetAccountByUserName(string userName);

    Task<Either<AccountError, List<Account>>> GetAccountsActiveAfterDate(DateTimeOffset date, int page = 1,
        int pageSize = 100);

    Task<Option<AccountError>> AddRolesToAccount(string id, List<Role> roles);
    Task<Option<AccountError>> SetUpAdminAccount(string accountId, LoginModel login);

    Task<Option<AccountError>> ChangePassword(LoginModel login, string newPassword);

    Task<Option<AccountError>> BanAccount(string id, TimeSpan? duration);

    Task<Option<AccountError>> UnBanAccount(string id);

    Task<Option<AccountError>> RemoveRolesFromAccount(string id, List<Role> roles);
}