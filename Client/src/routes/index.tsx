import { createFileRoute } from "@tanstack/react-router";
import { routeGuard } from "@/contexts/AuthContext";
import { SidebarInset } from "@/components/ui/sidebar";
import FLAdminSidebar from "@/components/FLAdminSidebar";
import DashboardGraph from "@/components/Dashboard/DashboardGraph";
import ChatWindow from "@/components/Dashboard/ChatWindow";
import SettingsBoard from "@/components/Dashboard/SettingsBoard";

export const Route = createFileRoute("/")({
  beforeLoad: routeGuard,
  component: Dashboard,
});

function Dashboard() {
  return (
    <main className="h-screen w-screen flex">
      <FLAdminSidebar />
      <SidebarInset className="flex-1 p-5 grid grid-cols-3 grid-rows-3 gap-5">
        <DashboardGraph />
        <ChatWindow />
        <SettingsBoard />
      </SidebarInset>
    </main>
  );
}
