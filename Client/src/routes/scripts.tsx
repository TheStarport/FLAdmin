import { createFileRoute } from "@tanstack/react-router";
import FLAdminSidebar from "@/components/FLAdminSidebar";
import { SidebarInset } from "@/components/ui/sidebar";

export const Route = createFileRoute("/scripts")({
  component: RouteComponent,
});

function RouteComponent() {
  return (
    <main className="flex">
      <FLAdminSidebar />
      <SidebarInset className="flex-1">
        <div>Hello from scripts!</div>
      </SidebarInset>
    </main>
  );
}
