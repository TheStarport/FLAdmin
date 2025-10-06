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
    <main className="h-screen w-screen flex items-center justify-center">
      <SetupCard />
    </main>
  );
}
