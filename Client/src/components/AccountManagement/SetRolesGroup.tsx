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

interface SetRolesGroupProps {
  accountRoles: string[];
  validRoles: string[];
  onSetRole: (role: string) => void;
}

function SetRolesGroup({
  accountRoles,
  validRoles,
  onSetRole,
}: SetRolesGroupProps) {
  const listRoleDropdownItems: ReactNode[] = validRoles.map(
    (validRole: string, index: number) => {
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
    (accountRole: string, index: number) => {
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
    <Card>
      <CardHeader>
        <CardTitle>Account roles</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-row gap-2 items-start">
        <DropdownMenu>
          <DropdownMenuTrigger>
            <Button>Change Roles</Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent>{listRoleDropdownItems}</DropdownMenuContent>
        </DropdownMenu>
        <div className="flex gap-1 flex-wrap">{listAccountRoleBadges}</div>
      </CardContent>
    </Card>
  );
}

export default SetRolesGroup;
