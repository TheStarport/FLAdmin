import { createFileRoute } from "@tanstack/react-router";
import { routeGuard } from "@/contexts/AuthContext";
import FLAdminSidebar from "@/components/FLAdminSidebar";
import { SidebarInset } from "@/components/ui/sidebar";
import AccountTable from "@/components/AccountManagement/AccountTable";
import { accountColumns } from "@/components/AccountManagement/AccountTableColumnDef";
import useData from "@/contexts/UseData";
import NoDataComponent from "@/components/NoDataComponent";
import ErrorDataComponent from "@/components/ErrorDataComponent";

export const Route = createFileRoute("/accounts")({
  beforeLoad: routeGuard,
  component: AccountsPage,
});

function AccountsPage() {
  const { accountData, accountLoading, accountError } = useData();

  return (
    <main className="flex h-screen w-screen">
      <FLAdminSidebar />
      <SidebarInset className="flex-1">
        <div className="flex-1 flex p-5">
          {accountLoading ? (
            <div>Loading...</div>
          ) : accountError ? (
            <ErrorDataComponent />
          ) : accountData ? (
            <AccountTable columns={accountColumns} data={accountData} />
          ) : (
            <NoDataComponent />
          )}
        </div>
      </SidebarInset>
    </main>
  );
}
