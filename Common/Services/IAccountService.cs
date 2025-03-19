using FlAdmin.Common.Models.Auth;
using FlAdmin.Common.Models.Database;
using FlAdmin.Common.Models.Error;
using LanguageExt;

namespace FlAdmin.Common.Services;

public interface IAccountService
{
    /// <summary>
    /// </summary>
    /// <param name="pageCount"> The page to be retrieved.</param>
    /// <param name="pageSize">How large each page is.</param>
    /// <returns>Returns a list of accounts </returns>
    Task<List<Account>> GetAccounts(int pageCount, int pageSize);


    List<Account> QueryAccounts(IQueryable<Account> query);


    /// <summary>
    ///     Gets an account that matches the provided id.
    /// </summary>
    /// <param name="id"> Id to match against</param>
    /// <returns>
    ///     A "Left/Right" Either object, Left being an error if the operation was a failure, Right being the account if
    ///     the operation was successful.
    /// </returns>
    Task<Either<FLAdminError, Account>> GetAccountById(string id);

    /// <summary>
    ///     Creates the provided accounts on the Database.
    /// </summary>
    /// <param name="accounts">List of accounts to be added to the database.</param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminError>> CreateAccounts(params Account[] accounts);


    /// <summary>
    ///     Replaces an account of the matching Id with the one provided.
    /// </summary>
    /// <param name="account"></param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminError>> UpdateAccount(Account account);


    /// <summary>
    ///     Deletes the accounts on the database that match the provided Ids.
    /// </summary>
    /// <param name="ids"> List of Ids to be deleted.</param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminError>> DeleteAccounts(params string[] ids);

    /// <summary>
    ///     Updates an Account's field, Type-safety of empty lists is not guaranteed so exercise caution when updating fields
    ///     with lists.
    /// </summary>
    /// <param name="accountId">Id of the account.</param>
    /// <param name="name">Name of the field to be updated.</param>
    /// <param name="value">Value of the field to be updated.</param>
    /// <typeparam name="T">Type of the field to be updated.</typeparam>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminError>> UpdateFieldOnAccount<T>(string accountId, string name, T value);

    /// <summary>
    ///     Gets all accounts that are actively logged into the game.
    /// </summary>
    /// <returns>List of accounts that are logged into the game.</returns>
    List<Account> GetOnlineAccounts();

    /// <summary>
    ///     Sets up the WebMaster/SuperAdmin for the server. Only ran once and will error if ran with one already present.
    /// </summary>
    /// <param name="loginModel"> Username and password of the SuperAdmin</param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminError>> CreateWebMaster(LoginModel loginModel);

    /// <summary>
    ///     Gets the account based on the username associated with the account
    /// </summary>
    /// <param name="userName"></param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Either<FLAdminError, Account>> GetAccountByUserName(string userName);


    /// <summary>
    ///     Returns a paginated list of all accounts that have logged into the game after the provided date
    /// </summary>
    /// <param name="date">The date</param>
    /// <param name="page">Page number, default is first. </param>
    /// <param name="pageSize">Size of the pagination, default is 100</param>
    /// <returns>
    ///     A "Left/Right" Either object, Left being an error if the operation was a failure, Right being the List of
    ///     accounts if the operation was successful.
    /// </returns>
    Task<Either<FLAdminError, List<Account>>> GetAccountsActiveAfterDate(DateTimeOffset date, int page = 1,
        int pageSize = 100);


    /// <summary>
    ///     Adds specified roles to the account. Some roles such as SuperAdmin are protected and cannot be added.
    /// </summary>
    /// <param name="id">Id of the account.</param>
    /// <param name="roles">List of roles to be added.</param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminError>> AddRolesToAccount(string id, List<Role> roles);


    /// <summary>
    ///     Sets up a Freelancer account with an Admin Account on the Database.
    /// </summary>
    /// <param name="accountId">Freelancer Account Id the admin account is to be associated with</param>
    /// <param name="login">Username and Password for the admin.</param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminError>> SetUpAdminAccount(string accountId, LoginModel login);

    /// <summary>
    ///     Changes password of a given username.
    /// </summary>
    /// <param name="login"> The login including the username and old password</param>
    /// <param name="newPassword">The new password</param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminError>> ChangePassword(LoginModel login, string newPassword);

    /// <summary>
    ///     Bans an account either permanently or temporarily based on the inclusion of a duration
    /// </summary>
    /// <param name="id"> id of the account to be banned.</param>
    /// <param name="duration"> Optional Duration of the ban, providing none will make the ban permanent </param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminError>> BanAccount(string id, TimeSpan? duration);

    /// <summary>
    ///     Unbans the specified account
    /// </summary>
    /// <param name="id">Account to be unbanned</param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminError>> UnBanAccount(string id);

    /// <summary>
    ///     Removes roles from a specified account, roles such as SuperAdmin are protected and unable to be removed.
    /// </summary>
    /// <param name="id"> Id of the account.</param>
    /// <param name="roles"> List of roles to Remove.</param>
    /// <returns>
    ///     Optional error enum, None means the operation was successful, Some means an error was encountered and the
    ///     operation failed.
    /// </returns>
    Task<Option<FLAdminError>> RemoveRolesFromAccount(string id, List<Role> roles);
}