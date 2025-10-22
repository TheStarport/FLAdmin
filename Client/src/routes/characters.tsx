import { createFileRoute } from "@tanstack/react-router";
import { routeGuard } from "@/contexts/AuthContext";
import FLAdminSidebar from "@/components/FLAdminSidebar";
import { SidebarInset } from "@/components/ui/sidebar";
import ErrorDataComponent from "@/components/ErrorDataComponent";
import NoDataComponent from "@/components/NoDataComponent";
import CharactersTable from "@/components/CharacterManagement/CharactersTable";
import useData from "@/contexts/UseData";
import { charactersColumns } from "@/components/CharacterManagement/CharactersTableColumnDef";

export const Route = createFileRoute("/characters")({
  beforeLoad: routeGuard,
  component: CharactersPage,
});

function CharactersPage() {
  const { charactersData, charactersLoading, charactersError } = useData();

  return (
    <main className="flex h-screen w-screen">
      <FLAdminSidebar />
      <SidebarInset className="flex-1">
        <div className="flex-1 flex p-5">
          {charactersLoading ? (
            <div>Loading...</div>
          ) : charactersError ? (
            <ErrorDataComponent />
          ) : charactersData ? (
            <CharactersTable
              columns={charactersColumns}
              data={charactersData}
            />
          ) : (
            <NoDataComponent />
          )}
        </div>
      </SidebarInset>
    </main>
  );
}
