import { SidebarProvider } from "@/components/ui/sidebar";
import { Outlet, createRootRoute } from "@tanstack/react-router";

export const Route = createRootRoute({
  component: RootComponent,
});

function RootComponent() {
  return (
    <div className="h-screen w-screen">
      <SidebarProvider>
        <Outlet />
      </SidebarProvider>
    </div>
  );
}
