import { createFileRoute } from "@tanstack/react-router";
import { routeGuard } from "@/contexts/AuthContext";

export const Route = createFileRoute("/")({
  beforeLoad: routeGuard,
  component: Dashboard,
});

function Dashboard() {
  return <div>Hello from Dashboard!</div>;
}
