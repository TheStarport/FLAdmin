export type FLAdminRole =
  | "Web"
  | "Game"
  | "ManageAdmins"
  | "ManageAutomation"
  | "ManageAccounts"
  | "ManageServer"
  | "SuperAdmin"
  | "ManageRoles"
  | "Database";

//TODO Order by 'importance'
export const FLAdminRoles: FLAdminRole[] = [
  "SuperAdmin",
  "ManageAdmins",
  "ManageAutomation",
  "ManageAccounts",
  "ManageServer",
  "ManageRoles",
  "Database",
  "Web",
  "Game",
];
