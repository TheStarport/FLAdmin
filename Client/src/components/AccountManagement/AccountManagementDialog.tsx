import {
  Dialog,
  DialogContent,
  DialogTrigger,
  DialogHeader,
} from "../ui/dialog";
import { Input } from "../ui/input";
import { Button } from "../ui/button";
import { EllipsisVerticalIcon } from "lucide-react";
import { DialogDescription } from "@radix-ui/react-dialog";
import { Separator } from "../ui/separator";

interface AccountManagementDialogProps {
  accountRoles: string[]; // Roles the account already has
  validRoles: string[]; // Roles an account could potentially have
}

function AccountManagementDialog({
  accountRoles,
  validRoles,
}: AccountManagementDialogProps) {
  return (
    <Dialog>
      <DialogTrigger>
        <Button>
          <EllipsisVerticalIcon />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>Manage Account</DialogHeader>
        <DialogDescription>Change this account's properties.</DialogDescription>
        <Input placeholder="New Username"></Input>
        <div className="flex flex-row gap-2">
          <Input placeholder="Password"></Input>
          <Input placeholder="Confirm Password"></Input>
        </div>
        <Separator />
      </DialogContent>
    </Dialog>
  );
}

export default AccountManagementDialog;
