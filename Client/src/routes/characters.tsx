import {
  createFileRoute,
  redirect,
} from "@tanstack/react-router";
import { getCookie } from "typescript-cookie";

export const Route = createFileRoute("/characters")({
  beforeLoad: () => {
    const token = getCookie("flAdminToken");
    if (!token) {
      throw redirect({ to: "/login" });
    }
  },
  component: CharactersPage,
});

function CharactersPage() {
  return <div>Hello from Characters!</div>;
}
