import axios from "axios";
import type Account from "@/types/account";

const fladminClient = axios.create({
  headers: {
    "Content-Type": "application/json",
  },
});

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
