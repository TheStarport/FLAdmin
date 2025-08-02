import {
  Dialog,
  DialogHeader,
  DialogTrigger,
  DialogContent,
  DialogDescription,
  DialogClose,
} from "../ui/dialog";
import { Button } from "../ui/button";
import { useState } from "react";
import { ChevronDownIcon } from "lucide-react";
import { Calendar } from "../ui/calendar";
import { Input } from "../ui/input";
import { Label } from "../ui/label";
import { Popover, PopoverContent, PopoverTrigger } from "../ui/popover";
import { banAccount } from "@/services/fladmin";
import { toast } from "sonner";

interface BanAccountDialogProps {
  banningAccountIds: string[];
}

function BanAccountDialog({ banningAccountIds }: BanAccountDialogProps) {
  const [dialogOpen, setDialogOpen] = useState<boolean>(false);
  const [calendarOpen, setCalendarOpen] = useState<boolean>(false);
  const [date, setDate] = useState<Date | undefined>(undefined);
  const [time, setTime] = useState<string>("10:30:00");
  const [inProgress, setInProgress] = useState<boolean>(false);

  const convertDateToTimespan = (
    unbanDate: Date,
    timeString: string
  ): number => {
    const [hours, minutes, seconds] = timeString.split(":").map(Number);
    const fullDate = new Date(unbanDate);
    fullDate.setHours(hours, minutes, seconds, 0);
    return fullDate.getTime() - Date.now();
  };

  const onClickConfirmBan = async (): Promise<void> => {
    setInProgress(true);
    const failedAccountIds: string[] = [];

    // We can assert Date exists, because otherwise the button would be disabled.
    const duration = convertDateToTimespan(date!, time);

    for (const accountId of banningAccountIds) {
      try {
        const res = await banAccount(accountId, duration);
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
            Some error occured while banning the following accounts:
            <br />
            <br />
            {failedAccountIds.map((id, index) => (
              <div key={index}>{`${index + 1}. ${id}`}</div>
            ))}
            <br />
            Please try again or contact your maintainer.
          </div>
        ) : (
          "Some error occured while banning many account(s). Please try again or contact the maintainer."
        );
      toast.error(
        `Failed to ban ${failedAccountIds.length} out of ${banningAccountIds.length}`,
        {
          description: errorDescription,
        }
      );
    } else {
      toast.success(`Successfully banned all selected accounts.`, {
        description: `Banned selected accounts until ${date?.toLocaleDateString()} at ${time}`,
      });
    }

    setInProgress(false);
    setDialogOpen(false);
  };

  return (
    <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
      <DialogTrigger>
        <Button variant="destructive">Ban Account</Button>
      </DialogTrigger>
      {inProgress ? (
        <DialogContent>
          <DialogHeader>Confirm Ban</DialogHeader>
          <DialogDescription>Banning accounts...</DialogDescription>
        </DialogContent>
      ) : (
        <DialogContent>
          <DialogHeader>Confirm Ban</DialogHeader>
          <DialogDescription>
            Please select a time until which the account(s) should be banned to
            confirm.
          </DialogDescription>
          <div className="flex gap-4 justify-center">
            <div className="flex flex-col gap-3">
              <Label htmlFor="date-picker" className="px-1">
                Date
              </Label>
              <Popover open={calendarOpen} onOpenChange={setCalendarOpen}>
                <PopoverTrigger asChild>
                  <Button
                    variant="outline"
                    id="date-picker"
                    className="w-32 justify-between font-normal"
                  >
                    {date ? date.toLocaleDateString() : "Select date"}
                    <ChevronDownIcon />
                  </Button>
                </PopoverTrigger>
                <PopoverContent
                  className="w-auto overflow-hidden p-0"
                  align="start"
                >
                  <Calendar
                    mode="single"
                    selected={date}
                    captionLayout="dropdown"
                    onSelect={(date) => {
                      setDate(date);
                      setCalendarOpen(false);
                    }}
                  />
                </PopoverContent>
              </Popover>
            </div>
            <div className="flex flex-col gap-3">
              <Label htmlFor="time-picker" className="px-1">
                Time
              </Label>
              <Input
                type="time"
                id="time-picker"
                step="1"
                value={time}
                onChange={(e) => setTime(e.target.value)}
                className="bg-background appearance-none [&::-webkit-calendar-picker-indicator]:hidden [&::-webkit-calendar-picker-indicator]:appearance-none"
              />
            </div>
          </div>
          <div className="flex flex-row justify-end gap-2 mt-4">
            <DialogClose>
              <Button>Cancel</Button>
            </DialogClose>
            <Button
              disabled={!date}
              variant="destructive"
              onClick={onClickConfirmBan}
            >
              Confirm Ban
            </Button>
          </div>
        </DialogContent>
      )}
    </Dialog>
  );
}

export default BanAccountDialog;
