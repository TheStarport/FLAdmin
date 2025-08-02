import axios from "axios";
import type Account from "@/types/account";

const fladminClient = axios.create({
  headers: {
    "Content-Type": "application/json",
  },
});

/* Account Controler */

export const getAccountById = (id: string) =>
  fladminClient.get<Account>(`/api/accounts/${id}`);

export const deleteAccounts = (ids: string[]) =>
  fladminClient.delete<string>("/api/accounts/delete", { data: ids });

export const banAccount = (accountId: string, duration?: number) =>
  fladminClient.patch<string>("/api/accounts/ban", null, {
    params: { accountId, duration },
  });

export const unbanAccount = (accountId: string) =>
  fladminClient.patch<string>("/api/accounts/unban", { data: accountId });

/* Character Controler */

export const removeAllCharactersFromAccount = (accountId: string) =>
  fladminClient.delete<string>("/api/characters/removeallfromaccount", {
    data: accountId,
  });
