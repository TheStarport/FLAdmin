namespace FlAdmin.Common.Models.Auth;

public enum Role : uint
{
    Web,
    ManageAdmins,
    ManageAutomation,
    ManageAccounts,
    SuperAdmin, // Essentially has all roles.
    ManageRoles
}