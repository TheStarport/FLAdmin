import { createFileRoute } from "@tanstack/react-router";
import { routeGuard } from "@/contexts/AuthContext";

export const Route = createFileRoute("/accounts")({
  beforeLoad: routeGuard,
  component: AccountsPage,
});

function AccountsPage() {
  return <div>Hello from Accounts!</div>;
}
