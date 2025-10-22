import type { Character } from "@/types/character";
import type { ColumnDef } from "@tanstack/react-table";
import { Checkbox } from "@radix-ui/react-checkbox";
import { Badge } from "../ui/badge";
import { Button } from "../ui/button";
import { ArrowUpDownIcon } from "lucide-react";

export const charactersColumns: ColumnDef<Character>[] = [
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
    id: "characterName",
    accessorKey: "characterName",
    sortingFn: (rowA, rowB) => {
      return rowA.original.characterName.localeCompare(
        rowB.original.characterName
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
          Character Name <ArrowUpDownIcon />
        </Button>
      );
    },
  },
  {
    accessorKey: "accountId",
    header: "Account ID",
  },
  {
    id: "onlineStatus",
    accessorFn: (row) => (row.onlineStatus ? 1 : 0),
    sortingFn: (rowA, rowB) => {
      const aIsOnline = rowA.original.onlineStatus ? 1 : 0;
      const bIsOnline = rowB.original.onlineStatus ? 1 : 0;
      return bIsOnline - aIsOnline;
    },
    header: ({ column }) => {
      return (
        <Button
          variant="ghost"
          onClick={() => {
            column.toggleSorting(column.getIsSorted() === "asc");
          }}
        >
          Online Status <ArrowUpDownIcon />
        </Button>
      );
    },
    cell: ({ row }) =>
      row.original.onlineStatus ? (
        <Badge>Online</Badge>
      ) : (
        <Badge variant="destructive">Offline</Badge>
      ),
  },
];
