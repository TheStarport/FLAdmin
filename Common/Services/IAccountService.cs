using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;

namespace FlAdmin.Common.Services;

public interface IAccountService
{
    Task<List<Account>> GetAllAccounts();

    List<Account> QueryAccounts(IQueryable<Account> query);

    Task<Either<FLAdminError, Account>> GetAccountById(string id);

    Task<Option<FLAdminError>> CreateAccounts(params Account[] accounts);
    Task<Option<FLAdminError>> UpdateAccount(Account account);
    Task<Option<FLAdminError>> DeleteAccounts(params string[] ids);

    Task<Option<FLAdminError>> UpdateFieldOnAccount<T>(string accountId, string name, T value);

    List<Account> GetOnlineAccounts();
    Task<Option<FLAdminError>> CreateWebMaster(LoginModel loginModel);

    Task<Either<FLAdminError, Account>> GetAccountByUserName(string userName);

    Task<Either<FLAdminError, List<Account>>> GetAccountsActiveAfterDate(DateTimeOffset date, int page = 1,
        int pageSize = 100);

    Task<Option<FLAdminError>> AddRolesToAccount(string id, List<Role> roles);
    Task<Option<FLAdminError>> SetUpAdminAccount(string accountId, LoginModel login);

    Task<Option<FLAdminError>> ChangePassword(LoginModel login, string newPassword);

    Task<Option<FLAdminError>> BanAccount(string id, TimeSpan? duration);

    Task<Option<FLAdminError>> UnBanAccount(string id);

    Task<Option<FLAdminError>> RemoveRolesFromAccount(string id, List<Role> roles);
}