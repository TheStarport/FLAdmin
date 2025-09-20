import { createFileRoute, redirect } from "@tanstack/react-router";
import { isSetup } from "@/services/fladmin";

export const Route = createFileRoute("/setup")({
  beforeLoad: async () => {
    const res = await isSetup();
    if (res.status === 200) throw redirect({ to: "/login" });
  },
  component: SetupPage,
});

function SetupPage() {
  return <div>Hello from Setup!</div>;
}
