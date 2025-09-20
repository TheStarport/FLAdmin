import { createFileRoute } from "@tanstack/react-router";
import { routeGuard } from "@/contexts/AuthContext";

export const Route = createFileRoute("/plugin_manager")({
  beforeLoad: routeGuard,
  component: PluginsPage,
});

function PluginsPage() {
  return <div>Hello from Plugins!</div>;
}
