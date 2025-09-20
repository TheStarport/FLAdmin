import { createFileRoute, redirect } from "@tanstack/react-router";
import { isSetup } from "@/services/fladmin";
import SetupCard from "@/components/SetupCard";

export const Route = createFileRoute("/setup")({
  beforeLoad: async () => {
    const res = await isSetup();
    if (res.status === 200 && res.data === true)
      throw redirect({ to: "/login" });
  },
  component: SetupPage,
});

function SetupPage() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <SetupCard />
    </div>
  );
}
