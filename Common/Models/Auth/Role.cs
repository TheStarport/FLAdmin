namespace FlAdmin.Common.Models.Auth;

public enum Role : uint
{
    Web,
    Game,
    ManageAdmins,
    ManageAutomation,
    ManageAccounts,
    ManageServer,
    SuperAdmin, // Essentially has all roles.
    ManageRoles,
    Database,
    User //Default role, basically anything that has a password will have this role.
}