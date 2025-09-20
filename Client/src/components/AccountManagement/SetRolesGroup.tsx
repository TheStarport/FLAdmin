import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from "../ui/dropdown-menu";
import { Card, CardContent, CardHeader, CardTitle } from "../ui/card";
import { Button } from "../ui/button";
import type { ReactNode } from "react";
import { Badge } from "../ui/badge";
import type { FLAdminRole } from "@/types/roles";

interface SetRolesGroupProps {
  accountRoles: FLAdminRole[];
  validRoles: FLAdminRole[];
  onSetRole: (role: FLAdminRole) => void;
}

function SetRolesGroup({
  accountRoles,
  validRoles,
  onSetRole,
}: SetRolesGroupProps) {
  const listRoleDropdownItems: ReactNode[] = validRoles.map(
    (validRole: FLAdminRole, index: number) => {
      return (
        <DropdownMenuCheckboxItem
          key={index}
          checked={accountRoles.includes(validRole)}
          onCheckedChange={() => onSetRole(validRole)}
        >
          {validRole}
        </DropdownMenuCheckboxItem>
      );
    }
  );

  const listAccountRoleBadges: ReactNode[] = accountRoles.map(
    (accountRole: FLAdminRole, index: number) => {
      return (
        <Badge
          key={index}
          variant="destructive"
          onClick={() => onSetRole(accountRole)}
        >{`- ${accountRole}`}</Badge>
      );
    }
  );

  return (
    <div className="flex flex-row content-start gap-4">
      <DropdownMenu>
        <DropdownMenuTrigger>
          <Button>Change Roles</Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent>{listRoleDropdownItems}</DropdownMenuContent>
      </DropdownMenu>
      <Card className="w-full">
        <CardHeader>
          <CardTitle>Account roles</CardTitle>
        </CardHeader>
        <CardContent className="flex flex-row gap-2 items-start">
          <div className="flex gap-1 flex-wrap">{listAccountRoleBadges}</div>
        </CardContent>
      </Card>
    </div>
  );
}

export default SetRolesGroup;
