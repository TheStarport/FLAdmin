import { createFileRoute } from "@tanstack/react-router";
import { routeGuard } from "@/contexts/AuthContext";
import { SidebarTrigger } from "@/components/ui/sidebar";
import FLAdminSidebar from "@/components/FLAdminSidebar";

export const Route = createFileRoute("/")({
  beforeLoad: routeGuard,
  component: Dashboard,
});

function Dashboard() {
  return (
    <main>
      <FLAdminSidebar />
      <div>
        <SidebarTrigger />
      </div>
    </main>
  );
}
