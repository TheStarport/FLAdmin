import { createFileRoute } from "@tanstack/react-router";
import { routeGuard } from "@/contexts/AuthContext";

export const Route = createFileRoute("/characters")({
  beforeLoad: routeGuard,
  component: CharactersPage,
});

function CharactersPage() {
  return <div>Hello from Characters!</div>;
}
