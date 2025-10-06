import { createContext, useState, useEffect, type ReactNode } from "react";
import { getCookie, setCookie, removeCookie } from "typescript-cookie";
import { redirect } from "@tanstack/react-router";
import { isSetup } from "@/services/fladmin";

type AuthContextType = {
  token: string | undefined;
  login: (token: string) => void;
  logout: () => void;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const routeGuard = async () => {
  if (import.meta.env.DEV) {
    return;
  }
  const setupResponse = await isSetup();
  if (!setupResponse.data) {
    throw redirect({ to: "/setup" });
  }
  const token = getCookie("flAdminToken");
  if (!token) {
    throw redirect({ to: "/login" });
  }
};

export const loginGuard = async () => {
  if (import.meta.env.DEV) {
    return;
  }
  const setupResponse = await isSetup();
  if (!setupResponse.data) {
    throw redirect({ to: "/setup" });
  }
  const token = getCookie("flAdminToken");
  if (token) {
    throw redirect({ to: "/" });
  }
};

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | undefined>(() =>
    getCookie("flAdminToken")
  );

  useEffect(() => {
    if (token) {
      setCookie("flAdminToken", token);
    } else {
      removeCookie("flAdminToken");
    }
  }, [token]);

  const login = (jwt: string) => setToken(jwt);
  const logout = () => setToken(undefined);

  return (
    <AuthContext.Provider value={{ token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export { AuthContext };
