namespace Common.Storage;

using System.Linq.Expressions;
using Auth;
using Models;
using Models.Database;
using Models.Forms;

public interface IAccountStorage
{
	Task<long> GetAccountCountAsync();
	Task<Pagination<Account>?> GetAccountsAsync(int page, int amountPerPage = 20, Expression<Func<Account, bool>>? filter = null);
	Task<Account?> GetAccountByIdAsync(string id);
	Task<Account?> CreateNewCharacterAsync(string accountId, Character character);
	Task<Character?> GetCharacterByNameAsync(string name);
	Task<Pagination<Account>?> SearchForCharacter(string characterName, int amountPerPage = 20);
	Task SetAccountToken(Account account, string? token);
	Task SetAccountRoles(Account account, IEnumerable<Role> webRoles, List<string> gameRoles);
	Task<bool> InstanceAdminExists();
	Task<string?> CreateInstanceAdmin(SignUp signUp);
	IQueryable<Account> GetAdmins();
}
