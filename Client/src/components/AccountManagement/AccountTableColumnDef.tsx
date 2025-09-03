import type Account from "@/types/account";
import type { ColumnDef } from "@tanstack/react-table";
import { FLAdminRoles, type FLAdminRole } from "@/types/roles";
import { Badge } from "../ui/badge";
import { Checkbox } from "../ui/checkbox";
import AccountManagementDialog from "./AccountManagementDialog";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { ArrowUpDownIcon, BanIcon } from "lucide-react";
import { Button } from "../ui/button";

export const accountColumns: ColumnDef<Account>[] = [
  {
    id: "select",
    header: ({ table }) => (
      <Checkbox
        checked={
          table.getIsAllPageRowsSelected() ||
          (table.getIsSomePageRowsSelected() && "indeterminate")
        }
        onCheckedChange={(value) => table.toggleAllPageRowsSelected(!!value)}
        aria-label="Select all"
      />
    ),
    cell: ({ row }) => (
      <Checkbox
        checked={row.getIsSelected()}
        onCheckedChange={(value) => row.toggleSelected(!!value)}
        aria-label="Select row"
      />
    ),
    enableSorting: false,
    enableHiding: false,
  },
  {
    accessorKey: "id",
    header: "ID",
  },
  //TODO implement online status following backend updates
  {
    id: "roles",
    cell: ({ row }) => {
      // Display the most 'important' game / web role
      const roles = FLAdminRoles.filter((role: FLAdminRole) => {
        return (
          row.original.gameRoles?.includes(role) ||
          row.original.webRoles?.includes(role)
        );
      });
      return roles.length ? (
        <div className="flex flex-row gap-1 overflow-hidden">
          {roles.map((role: FLAdminRole) => (
            <Badge key={role} variant="destructive">
              {role}
            </Badge>
          ))}
        </div>
      ) : (
        "No Roles"
      );
    },
    header: "Game Roles",
    filterFn: (row, _, filterValue) => {
      if (!filterValue || filterValue.length === 0) return true;
      return filterValue.every((role: FLAdminRole) => 
        row.original.gameRoles?.includes(role) || row.original.webRoles?.includes(role)
      );
    },
  },
  {
    id: "banStatus",
    accessorFn: (row) => (row.scheduledUnbanDate ? 1 : 0),
    sortingFn: (rowA, rowB) => {
      const aHasBan = rowA.original.scheduledUnbanDate ? 1 : 0;
      const bHasBan = rowB.original.scheduledUnbanDate ? 1 : 0;
      return bHasBan - aHasBan;
    },
    cell: ({ row }) => {
      return row.original.scheduledUnbanDate ? (
        <Tooltip>
          <TooltipTrigger>
            <BanIcon className="text-red-500" />
          </TooltipTrigger>
          <TooltipContent>
            <p>
              Until:{" "}
              {new Date(row.original.scheduledUnbanDate).toLocaleString()}
            </p>
          </TooltipContent>
        </Tooltip>
      ) : (
        <div></div>
      );
    },
    header: ({ column }) => {
      return (
        <Button
          variant="ghost"
          onClick={() => {
            column.toggleSorting(column.getIsSorted() === "asc");
          }}
        >
          Ban Status <ArrowUpDownIcon />
        </Button>
      );
    },
  },
  {
    accessorKey: "cash",
    cell: ({ row }) =>
      row.original.cash?.toLocaleString("en-US", {
        style: "currency",
        currency: "USD",
      }),
    header: ({ column }) => {
      return (
        <Button
          variant="ghost"
          onClick={() => {
            column.toggleSorting(column.getIsSorted() === "asc");
          }}
        >
          Cash <ArrowUpDownIcon />
        </Button>
      );
    },
  },
  {
    id: "actions",
    cell: ({ row }) => {
      return <AccountManagementDialog editingAccountId={row.original.id} />;
    },
  },
];
