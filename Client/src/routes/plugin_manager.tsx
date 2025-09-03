import { createFileRoute, redirect } from "@tanstack/react-router";
import { getCookie } from "typescript-cookie";

export const Route = createFileRoute("/plugin_manager")({
  beforeLoad: () => {
    const token = getCookie("flAdminToken");
    if (!token) {
      throw redirect({ to: "/login" });
    }
  },
  component: PluginsPage,
});

function PluginsPage() {
  return <div>Hello from Plugins!</div>;
}
