import { createFileRoute, redirect } from "@tanstack/react-router";
import { getCookie } from "typescript-cookie";

export const Route = createFileRoute("/accounts")({
  beforeLoad: () => {
    const token = getCookie("flAdminToken");
    if (!token) {
      throw redirect({ to: "/login" });
    }
  },
  component: AccountsPage,
});

function AccountsPage() {
  return <div>Hello from Accounts!</div>;
}
