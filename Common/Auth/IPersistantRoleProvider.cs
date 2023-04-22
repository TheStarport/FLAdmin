namespace Common.Auth;
public interface IPersistantRoleProvider
{
    AdminUser? GetUser(string name);
    void LoadUsers();
    void SaveUsers();
    void UpdateToken(string name, string token);
    void UpdateRoles(string name, IEnumerable<Role> roles);
    void AddUser(AdminUser user);
    void RemoveUser(string name);
}
