import {
  EllipsisVerticalIcon,
  ChevronRightIcon,
  ChevronLeftIcon,
} from "lucide-react";
import { Button } from "../ui/button";
import {
  Dialog,
  DialogTrigger,
  DialogContent,
  DialogHeader,
  DialogDescription,
  DialogClose,
} from "../ui/dialog";
import {
  Command,
  CommandEmpty,
  CommandInput,
  CommandItem,
  CommandList,
} from "../ui/command";
import { Separator } from "../ui/separator";
import { useState, type ReactNode } from "react";
import { Badge } from "../ui/badge";
import { moveCharacterToAccount } from "@/services/fladmin";
import { toast } from "sonner";

interface MoveCharacterDialogProps {
  characterIds: string[];
  accountIds: string[];
  pageSize: number;
}

function MoveCharacterDialog({
  characterIds,
  accountIds,
  pageSize,
}: MoveCharacterDialogProps) {
  const paginateAccountIds = (
    accountIdsArr: string[],
    pageSize: number
  ): string[][] => {
    const pages: string[][] = [];
    for (let i = 0; i < accountIdsArr.length; i += pageSize) {
      const page = accountIdsArr.slice(i, i + pageSize);
      pages.push(page);
    }
    return pages;
  };

  const [dialogOpen, setDialogOpen] = useState<boolean>(false);
  const [inProgress, setInProgress] = useState<boolean>(false);
  const [currentPageIndex, setCurrentPageIndex] = useState<number>(0);
  const [paginatedAccountIds, setPaginatedAccountIds] = useState<string[][]>(
    paginateAccountIds(accountIds, pageSize)
  );
  const [searchInput, setSearchInput] = useState<string>("");
  const [selectedAccount, setSelectedAccount] = useState<string>("");

  const listCommandItems = (pageIndex: number): ReactNode[] => {
    if (!paginatedAccountIds.length) {
      return [];
    }
    const accountIdsPage: ReactNode[] = [];
    paginatedAccountIds[pageIndex].forEach((accountId: string) => {
      accountIdsPage.push(
        <CommandItem
          key={accountId}
          value={accountId}
          onSelect={(currentValue) => {
            setSelectedAccount(
              currentValue === selectedAccount ? "" : currentValue
            );
          }}
        >
          {accountId}
        </CommandItem>
      );
    });
    return accountIdsPage;
  };

  const filterAccountIds = (newSearchValue: string): void => {
    setCurrentPageIndex(0);
    setSearchInput(newSearchValue);
    const filteredAccountIds: string[] = accountIds.filter(
      (accountId: string) => accountId.includes(newSearchValue)
    );
    setPaginatedAccountIds(paginateAccountIds(filteredAccountIds, pageSize));
  };

  const onClickMoveCharacter = async (): Promise<void> => {
    setInProgress(true);
    const failedCharacterIds: string[] = [];
    for (const characterId of characterIds) {
      try {
        const res = await moveCharacterToAccount(characterId, selectedAccount);
        if (!(res.status === 200)) failedCharacterIds.push(characterId);
      } catch {
        failedCharacterIds.push(characterId);
      }
    }

    if (!failedCharacterIds.length) {
      toast.success("Success moving characters.", {
        description: `Moved character(s) to ${selectedAccount}`,
      });
    } else if (failedCharacterIds.length <= 5) {
      const errorDescription: ReactNode = (
        <div>
          Some error occured while moving the following characters:
          <br />
          <br />
          {failedCharacterIds.map((id: string, index: number) => (
            <div key={index}>{`${index + 1}. ${id}`}</div>
          ))}
          <br />
          Please try again or contact your maintainer.
        </div>
      );
      toast.error("Error moving characters.", {
        description: errorDescription,
      });
    } else {
      toast.error("Error moving characters.", {
        description:
          "Some error occured while moving many characters. Please try again or contact your maintainer.",
      });
    }
    setInProgress(false);
    setDialogOpen(false);
  };

  return (
    <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
      <DialogTrigger>
        <Button>
          <EllipsisVerticalIcon />
        </Button>
      </DialogTrigger>
      {inProgress ? (
        <DialogContent>
          <DialogHeader>Move character(s) to a new Account</DialogHeader>
          <DialogDescription>
            Moving characters in progress...
          </DialogDescription>
        </DialogContent>
      ) : (
        <DialogContent>
          <DialogHeader>Move character(s) to a new Account</DialogHeader>
          {selectedAccount.length ? (
            <DialogDescription className="flex flex-row gap-2">
              <span>Selected Account:</span>
              <Badge>{selectedAccount}</Badge>
            </DialogDescription>
          ) : (
            <DialogDescription>
              Please select an account to move the character to.
            </DialogDescription>
          )}

          <Command>
            <CommandInput
              placeholder="Search for an Account..."
              value={searchInput}
              onValueChange={(e) => filterAccountIds(e)}
            />
            <CommandList>
              <CommandEmpty>No Accounts found.</CommandEmpty>
              {listCommandItems(currentPageIndex)}
            </CommandList>
          </Command>
          <div className="flex flex-row justify-between items-center">
            <Button
              variant="secondary"
              disabled={currentPageIndex === 0}
              onClick={() => setCurrentPageIndex(currentPageIndex - 1)}
            >
              <ChevronLeftIcon />
            </Button>
            <span>{`Page ${currentPageIndex + 1} / ${
              paginatedAccountIds.length
            }`}</span>
            <Button
              variant="secondary"
              disabled={currentPageIndex === paginatedAccountIds.length - 1}
              onClick={() => setCurrentPageIndex(currentPageIndex + 1)}
            >
              <ChevronRightIcon />
            </Button>
          </div>
          <Separator />
          <div className="flex flex-row justify-end gap-2">
            <Button
              disabled={selectedAccount === ""}
              onClick={onClickMoveCharacter}
            >
              Move Character
            </Button>
            <DialogClose>
              <Button variant="secondary">Cancel</Button>
            </DialogClose>
          </div>
        </DialogContent>
      )}
    </Dialog>
  );
}

export default MoveCharacterDialog;
