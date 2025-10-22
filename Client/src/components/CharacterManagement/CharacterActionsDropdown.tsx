import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from "@radix-ui/react-dropdown-menu";
import { Button } from "../ui/button";
import { EllipsisVerticalIcon } from "lucide-react";
import RenameCharacterDialog from "./RenameCharacterDialog";
import { useState } from "react";
import { DropdownMenuItem } from "../ui/dropdown-menu";
import type { Character } from "@/types/character";

interface CharacterActionsDropdownProps {
  character: Character;
}

const CharacterActionsDropdown = ({
  character,
}: CharacterActionsDropdownProps) => {
  const [renameCharacterDialogOpen, setRenameCharacterDialogOpen] =
    useState<boolean>(false);

  return (
    <>
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="outline">
            <EllipsisVerticalIcon />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="start">
          <DropdownMenuItem
            onSelect={() => {
              setRenameCharacterDialogOpen(true);
            }}
          >
            Rename Character
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      <RenameCharacterDialog
        oldCharacterName={character.characterName}
        dialogOpen={renameCharacterDialogOpen}
        setDialogOpen={setRenameCharacterDialogOpen}
      />
    </>
  );
};

export default CharacterActionsDropdown;
