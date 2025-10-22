import axios from "axios";
import type Account from "@/types/account";

const fladminClient = axios.create({
  baseURL: await import.meta.env.VITE_FLADMIN_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

// Handle network errors gracefully
fladminClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Return a rejected promise instead of throwing
    return Promise.reject(error);
  }
);

/* Account Controler */

export const getAccountById = (id: string) =>
  fladminClient.get<Account>(`/api/accounts/${id}`, { timeout: 5000 });

export const deleteAccounts = (ids: string[]) =>
  fladminClient.delete<string>("/api/accounts/delete", {
    data: ids,
    timeout: 5000,
  });

export const banAccount = (accountId: string, duration?: number) =>
  fladminClient.patch<string>("/api/accounts/ban", null, {
    params: { accountId, duration },
    timeout: 5000,
  });

export const unbanAccount = (accountId: string) =>
  fladminClient.patch<string>("/api/accounts/unban", {
    data: accountId,
    timeout: 5000,
  });

/* Character Controler */

export const removeAllCharactersFromAccount = (accountId: string) =>
  fladminClient.delete<string>("/api/characters/removeallfromaccount", {
    data: accountId,
    timeout: 5000,
  });

export const moveCharacterToAccount = (
  characterName: string,
  newAccountId: string
) =>
  fladminClient.patch<string>("/api/characters/movecharactertoaccount", null, {
    params: { characterName, newAccountId },
    timeout: 5000,
  });

export const renameCharacter = (
  oldCharacterName: string,
  newCharacterName: string
) =>
  fladminClient.patch<string>("/api/characters/rename", null, {
    params: { oldName: oldCharacterName, newName: newCharacterName },
    timeout: 5000,
  });

/* Authentication Controler */

// TODO: Use returned Account
export const setup = (password: string) =>
  fladminClient.post<string>("/api/auth/setup", `"${password}"`, {
    timeout: 5000,
  });

// TODO endpoint could change
export const isSetup = async () => {
  try {
    const response = await fladminClient.get<boolean>("/api/auth/issetup", {
      timeout: 5000,
    });
    return response;
  } catch {
    // Return false on network errors - assume not setup
    // This is because Tanstack interprets redirects in 'catch' blocks as unhandled, and throws
    // a network error instead of rerouting
    return { data: false } as any;
  }
};

export const login = (username: string, password: string) =>
  fladminClient.post<string>("/api/auth/login", {
    data: { username, password },
    timeout: 5000,
  });
