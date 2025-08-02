import {
  Dialog,
  DialogContent,
  DialogTrigger,
  DialogHeader,
  DialogClose,
} from "../ui/dialog";
import { Input } from "../ui/input";
import { Button } from "../ui/button";
import {
  EllipsisVerticalIcon,
  ChevronsUpDownIcon,
  CircleXIcon,
  InfoIcon,
} from "lucide-react";
import { Separator } from "../ui/separator";
import SetRolesGroup from "./SetRolesGroup";
import { useEffect, useState } from "react";
import type Account from "@/types/account";
import { getAccountById } from "@/services/fladmin";
import { FLAdminRoles, type FLAdminRole } from "@/types/roles";
import {
  Collapsible,
  CollapsibleTrigger,
  CollapsibleContent,
} from "../ui/collapsible";
import BanAccountDialog from "./BanAccountDialog";

interface AccountManagementDialogProps {
  editingAccountId: string;
}

function AccountManagementDialog({
  editingAccountId,
}: AccountManagementDialogProps) {
  /* States relating to API */
  const [loading, setLoading] = useState<boolean>(false);
  const [editingAccount, setEditingAccount] = useState<Account | undefined>();
  const [errorState, setErrorState] = useState<boolean>(false);

  /* Editing fields */
  const [editingAccountRoles, setEditingAccountRoles] = useState<FLAdminRole[]>(
    []
  );

  useEffect(() => {
    const fetchAccount = async () => {
      try {
        setLoading(true);
        const accountData = await getAccountById(editingAccountId);

        setEditingAccount(accountData.data);

        const mergeRoles: FLAdminRole[] =
          accountData.data?.gameRoles && accountData.data?.webRoles
            ? [...accountData.data.gameRoles, ...accountData.data.webRoles]
            : [];

        setEditingAccountRoles(mergeRoles);
      } catch (err) {
        setErrorState(true);
      } finally {
        setLoading(false);
      }
    };

    fetchAccount();
  }, [editingAccountId]);

  return (
    <Dialog>
      <DialogTrigger>
        <Button>
          <EllipsisVerticalIcon />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <b>Manage Account</b>
        </DialogHeader>
        {errorState ? (
          /* Error State */
          <div className="flex flex-row gap-2 text-red-500">
            <CircleXIcon />
            <span>Something went wrong fetching this account's data.</span>
          </div>
        ) : loading ? (
          /* Loading State */
          <div className="flex flex-row gap-2">
            <InfoIcon />
            <span>Loading Account Data...</span>
          </div>
        ) : (
          /* Working component state */
          <div className="flex flex-col gap-4">
            <Input placeholder="New Username"></Input>
            <div className="flex flex-row gap-2">
              <Input placeholder="Password"></Input>
              <Input placeholder="Confirm Password"></Input>
            </div>
            <Separator />
            <SetRolesGroup
              accountRoles={editingAccountRoles}
              validRoles={FLAdminRoles}
              onSetRole={(role: FLAdminRole) =>
                setEditingAccountRoles((prev) =>
                  prev.includes(role)
                    ? prev.filter((r) => r !== role)
                    : [...prev, role]
                )
              }
            />
            <Separator />
            <Collapsible>
              <CollapsibleTrigger className="flex flex-row">
                <ChevronsUpDownIcon />
                Danger Zone
              </CollapsibleTrigger>
              <CollapsibleContent className="flex flex-row justify-between mt-2">
                <BanAccountDialog banningAccountIds={[editingAccountId]} />
                <Button disabled variant="secondary">
                  Unban Account
                </Button>
                <Button disabled variant="destructive">
                  Delete Account
                </Button>
                <Button disabled variant="destructive">
                  Remove Characters
                </Button>
              </CollapsibleContent>
            </Collapsible>
            <Separator />
            <div className="flex flex-row justify-end gap-4">
              <Button variant="default">Save</Button>
              <DialogClose>
                <Button variant={"secondary"}>Cancel</Button>
              </DialogClose>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}

export default AccountManagementDialog;
