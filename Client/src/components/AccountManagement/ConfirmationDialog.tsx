import {
  Dialog,
  DialogDescription,
  DialogHeader,
  DialogTrigger,
  DialogContent,
  DialogClose,
} from "../ui/dialog";
import { Button } from "../ui/button";
import {
  deleteAccounts,
  removeAllCharactersFromAccount,
  unbanAccount,
} from "@/services/fladmin";
import { toast } from "sonner";
import { useState } from "react";

type ConfirmationDialogVariant =
  | "Unban Account"
  | "Delete Account"
  | "Remove Characters";

interface ConfirmationDialogProps {
  variant: ConfirmationDialogVariant;
  editingAccountIds: string[];
}

/*
    TODO: Replace for-loop endpoints, once batch handling becomes availabled in the back-end.
*/
function ConfirmationDialog({
  variant,
  editingAccountIds,
}: ConfirmationDialogProps) {
  const [inProgress, setInProgress] = useState<boolean>(false);
  const [dialogOpen, setDialogOpen] = useState<boolean>(false);

  const onClickConfirmAction = async (): Promise<void> => {
    setInProgress(true);
    /* UI feedback for account deletion */
    if (variant === "Delete Account") {
      try {
        const res = await deleteAccounts(editingAccountIds);
        if (res.status === 200) {
          toast.success("Successfully deleted all selected accounts.", {
            description: "Deleted all selected accounts.",
          });
        } else {
          toast.error("Failed to delete accounts", {
            description: "Please try again or contact your maintainer.",
          });
        }
      } catch (error) {
        toast.error("Failed to delete accounts", {
          description:
            "Some error occured while deleting many account(s) Please try again of contact your maintainer.",
        });
      }

      /* UI feedback for character removal */
    } else if (variant === "Remove Characters") {
      const failedAccountIds: string[] = [];

      for (const accountId of editingAccountIds) {
        try {
          const res = await removeAllCharactersFromAccount(accountId);
          if (res.status !== 200) {
            failedAccountIds.push(accountId);
          }
        } catch (error) {
          failedAccountIds.push(accountId);
        }
      }

      if (failedAccountIds.length) {
        const errorDescription =
          failedAccountIds.length <= 5 ? (
            <div>
              Some error occured while removing characters from the following
              accounts:
              <br />
              <br />
              {failedAccountIds.map((id: string, index: number) => (
                <div key={index}>{`${index + 1}. ${id}`}</div>
              ))}
              <br />
              Please try again or contact your maintainer.
            </div>
          ) : (
            "Some error occured while removing characters from many account(s) Please try again of contact your maintainer."
          );

        toast.error(
          `Failed to remove characters from ${failedAccountIds.length} out of ${editingAccountIds.length}`,
          {
            description: errorDescription,
          }
        );
      } else {
        toast.success(
          "Successfully removed characters from all selected accounts.",
          {
            description: "Removed all characters from all accounts.",
          }
        );
      }

      /* UI feedback for Unban */
    } else {
      const failedAccountIds: string[] = [];

      for (const accountId of editingAccountIds) {
        try {
          const res = await unbanAccount(accountId);
          if (res.status !== 200) {
            failedAccountIds.push(accountId);
          }
        } catch (error) {
          failedAccountIds.push(accountId);
        }
      }

      if (failedAccountIds.length) {
        const errorDescription =
          failedAccountIds.length <= 5 ? (
            <div>
              Some error occured while unbaning the following accounts:
              <br />
              <br />
              {failedAccountIds.map((id: string, index: number) => (
                <div key={index}>{`${index + 1}. ${id}`}</div>
              ))}
              <br />
              Please try again or contact your maintainer.
            </div>
          ) : (
            "Some error occured while unbaning many account(s) Please try again of contact your maintainer."
          );

        toast.error(
          `Failed to remove characters from ${failedAccountIds.length} out of ${editingAccountIds.length}`,
          {
            description: errorDescription,
          }
        );
      } else {
        toast.success("Successfully unbaned all selected accounts.", {
          description: "Unban all accounts.",
        });
      }
    }

    setInProgress(false);
    setDialogOpen(false);
  };

  return (
    <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
      <DialogTrigger>
        <Button variant="destructive">{variant}</Button>
      </DialogTrigger>

      {!inProgress ? (
        <DialogContent>
          <DialogHeader>Confirm Action</DialogHeader>
          <DialogDescription>{`Are you sure you want to ${
            variant === "Delete Account"
              ? "delete this account?"
              : variant === "Unban Account"
              ? "unban this account?"
              : "remove all characters from this account?"
          }`}</DialogDescription>
          <div className="flex flex-row justify-end gap-2">
            <Button variant="destructive" onClick={onClickConfirmAction}>
              Confirm
            </Button>
            <DialogClose>
              <Button variant="secondary">Cancel</Button>
            </DialogClose>
          </div>
        </DialogContent>
      ) : (
        <DialogContent>
          <DialogHeader>Confirm Action</DialogHeader>
          <DialogDescription>
            {variant === "Unban Account"
              ? "Unbanning account(s) in progress..."
              : variant === "Delete Account"
              ? "Deleting account(s) in progress..."
              : "Removing characters from account(s) in progress..."}
          </DialogDescription>
        </DialogContent>
      )}
    </Dialog>
  );
}

export default ConfirmationDialog;
