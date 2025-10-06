import { createFileRoute } from "@tanstack/react-router";
import { routeGuard } from "@/contexts/AuthContext";
import FLAdminSidebar from "@/components/FLAdminSidebar";
import { SidebarInset } from "@/components/ui/sidebar";

export const Route = createFileRoute("/characters")({
  beforeLoad: routeGuard,
  component: CharactersPage,
});

function CharactersPage() {
  return (
    <main className="flex">
      <FLAdminSidebar />
      <SidebarInset className="flex-1">
        <div>Hello from Characters!</div>
      </SidebarInset>
    </main>
  );
}
