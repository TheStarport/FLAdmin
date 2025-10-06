import LoginCard from "@/components/LoginCard";
import { createFileRoute } from "@tanstack/react-router";
import { loginGuard } from "@/contexts/AuthContext";

export const Route = createFileRoute("/login")({
  beforeLoad: loginGuard,
  component: LoginPage,
});

function LoginPage() {
  return (
    <main className="h-screen w-screen flex items-center justify-center">
      <LoginCard />
    </main>
  );
}
