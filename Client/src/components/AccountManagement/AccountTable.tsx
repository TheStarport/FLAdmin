import {
  type ColumnDef,
  flexRender,
  useReactTable,
  getCoreRowModel,
  getPaginationRowModel,
  type SortingState,
  getSortedRowModel,
  type ColumnFiltersState,
  getFilteredRowModel,
} from "@tanstack/react-table";
import {
  Table,
  TableHeader,
  TableRow,
  TableHead,
  TableBody,
  TableCell,
} from "../ui/table";
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "../ui/pagination";
import { useState } from "react";
import { Input } from "../ui/input";
import { FunnelIcon } from "lucide-react";
import { Button } from "../ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem,
} from "../ui/dropdown-menu";
import { FLAdminRoles, type FLAdminRole } from "@/types/roles";

interface AccountTableProps<TData, TValue> {
  columns: ColumnDef<TData, TValue>[];
  data: TData[];
}

function AccountTable<TData, TValue>({
  columns,
  data,
}: AccountTableProps<TData, TValue>) {
  const [sorting, setSorting] = useState<SortingState>([]);
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([]);
  const [selectedRoles, setSelectedRoles] = useState<FLAdminRole[]>([]);

  const accountTable = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    onSortingChange: setSorting,
    getSortedRowModel: getSortedRowModel(),
    onColumnFiltersChange: setColumnFilters,
    getFilteredRowModel: getFilteredRowModel(),
    state: {
      sorting,
      columnFilters,
    },
  });

  return (
    <div>
      <div className="flex flex-row gap-2 mb-2">
        <Input
          placeholder="Search for Account ID..."
          value={
            (accountTable.getColumn("id")?.getFilterValue() as string) ?? ""
          }
          onChange={(event) =>
            accountTable.getColumn("id")?.setFilterValue(event.target.value)
          }
        />
        <DropdownMenu>
          <DropdownMenuTrigger>
            <Button variant="outline">
              <FunnelIcon />
              Filter Roles
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent>
            {FLAdminRoles.map((role: FLAdminRole) => (
              <DropdownMenuCheckboxItem
                key={role}
                checked={selectedRoles.includes(role)}
                onClick={() => {
                  const newSelectedRoles = selectedRoles.includes(role)
                    ? selectedRoles.filter((r) => r !== role)
                    : [...selectedRoles, role];
                  setSelectedRoles(newSelectedRoles);
                  accountTable
                    .getColumn("roles")
                    ?.setFilterValue(
                      newSelectedRoles.length > 0 ? newSelectedRoles : undefined
                    );
                }}
              >
                {role}
              </DropdownMenuCheckboxItem>
            ))}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      <div className="overflow-hidden rounded-md border">
        <Table>
          <TableHeader>
            {accountTable.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => {
                  return (
                    <TableHead key={header.id}>
                      {header.isPlaceholder
                        ? null
                        : flexRender(
                            header.column.columnDef.header,
                            header.getContext()
                          )}
                    </TableHead>
                  );
                })}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {accountTable.getRowModel().rows?.length ? (
              accountTable.getRowModel().rows.map((row) => (
                <TableRow
                  key={row.id}
                  data-state={row.getIsSelected() && "selected"}
                >
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext()
                      )}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell
                  colSpan={columns.length}
                  className="h-24 text-center"
                >
                  No results.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
        <Pagination className="justify-start">
          <PaginationContent className="flex flex-row justify-between w-full">
            <PaginationItem>
              <PaginationPrevious
                href="#"
                onClick={(e) => {
                  e.preventDefault();
                  accountTable.previousPage();
                }}
                className={
                  !accountTable.getCanPreviousPage()
                    ? "pointer-events-none opacity-50"
                    : ""
                }
              />
            </PaginationItem>
            <div className="flex items-center gap-1">
              {(() => {
                const currentPage =
                  accountTable.getState().pagination.pageIndex;
                const totalPages = accountTable.getPageCount();
                const maxVisible = 5;

                if (totalPages <= maxVisible) {
                  return Array.from({ length: totalPages }, (_, i) => (
                    <PaginationItem key={i}>
                      <PaginationLink
                        href="#"
                        onClick={(e) => {
                          e.preventDefault();
                          accountTable.setPageIndex(i);
                        }}
                        isActive={currentPage === i}
                      >
                        {i + 1}
                      </PaginationLink>
                    </PaginationItem>
                  ));
                }

                const start = Math.max(0, currentPage - 2);
                const end = Math.min(totalPages, start + maxVisible);

                return Array.from({ length: end - start }, (_, i) => {
                  const pageIndex = start + i;
                  return (
                    <PaginationItem key={pageIndex}>
                      <PaginationLink
                        href="#"
                        onClick={(e) => {
                          e.preventDefault();
                          accountTable.setPageIndex(pageIndex);
                        }}
                        isActive={currentPage === pageIndex}
                      >
                        {pageIndex + 1}
                      </PaginationLink>
                    </PaginationItem>
                  );
                });
              })()}
            </div>
            <PaginationItem>
              <PaginationNext
                href="#"
                onClick={(e) => {
                  e.preventDefault();
                  accountTable.nextPage();
                }}
                className={
                  !accountTable.getCanNextPage()
                    ? "pointer-events-none opacity-50"
                    : ""
                }
              />
            </PaginationItem>
          </PaginationContent>
        </Pagination>
      </div>
    </div>
  );
}

export default AccountTable;
