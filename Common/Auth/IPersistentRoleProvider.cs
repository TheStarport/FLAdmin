namespace Common.Auth;
public interface IPersistentRoleProvider
{
	IEnumerable<AdminUser> GetUsers();
	AdminUser? GetUser(string name);
	public bool GenerateDefaultAdminUserIfNotExists();
	void LoadUsers();
	void SaveUsers();
	void UpdateToken(string name, string token);
	void UpdateRoles(string name, IEnumerable<Role> roles);
	void AddUser(AdminUser user);
	void RemoveUser(string name);
}
