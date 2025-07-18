namespace FlAdmin.Common.Models.Auth;

public enum Role : uint
{
    Web,
    ManageAdmins,
    ManageAutomation,
    ManageAccounts,
    ManageServer,
    SuperAdmin, // Essentially has all roles.
    ManageRoles,
    Database,
    
}