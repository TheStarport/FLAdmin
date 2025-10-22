import { Button } from "../ui/button";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
} from "../ui/dialog";
import { useState } from "react";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import { renameCharacter } from "@/services/fladmin";
import { toast } from "sonner";

interface RenameCharacterDialog {
  oldCharacterName: string;
  dialogOpen: boolean;
  setDialogOpen: (arg0: boolean) => void;
}

const RenameCharacterDialog = ({
  oldCharacterName,
  dialogOpen,
  setDialogOpen,
}: RenameCharacterDialog) => {
  const [newCharacterName, setNewCharacterName] = useState<string>("");

  const onClickRename = async () => {
    const res = await renameCharacter(oldCharacterName, newCharacterName);

    if (res.status === 200) {
      toast.success("Success renaming character.", {
        description: `Renamed character to ${newCharacterName}`,
      });
      setDialogOpen(false);
    } else {
      toast.error("Error renaming character.", {
        description:
          "Something went wrong renaming the character. Please try again or contact your maintainer.",
      });
    }
  };

  return (
    <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
      <DialogContent>
        <DialogHeader>Rename Character</DialogHeader>
        <div className="grid w-full max-w-sm items-center gap-3">
          <Label htmlFor="newName">Character's New Name</Label>
          <Input
            id="newName"
            placeholder="Character's new name"
            value={newCharacterName}
            onChange={(e) => {
              setNewCharacterName(e.target.value);
            }}
          />
        </div>
        <DialogFooter className="flex flex-row justify-end gap-2">
          <Button disabled={newCharacterName === ""} onClick={onClickRename}>
            Rename Character
          </Button>
          <DialogClose>
            <Button variant="secondary">Cancel</Button>
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

export default RenameCharacterDialog;
