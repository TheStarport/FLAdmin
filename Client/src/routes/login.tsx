import { createFileRoute, redirect } from "@tanstack/react-router";
import { getCookie } from "typescript-cookie";

export const Route = createFileRoute("/login")({
  beforeLoad: () => {
    const token = getCookie("flAdminToken");
    if (token) {
      throw redirect({ to: "/" });
    }
  },
  component: LoginPage,
});

function LoginPage() {
  return <div>Hello from Login!</div>;
}
