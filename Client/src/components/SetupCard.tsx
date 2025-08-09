import { Card, CardContent } from "./ui/card";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { setup } from "@/services/fladmin";
import { useState } from "react";
import { InfoIcon } from "lucide-react";

interface SetupCardProps {}

function SetupCard({}: SetupCardProps) {
  const [passwordFieldValue, setPasswordFieldValue] = useState<string>("");
  const [errorState, setErrorState] = useState<boolean>(false);

  const onClickSubmit = async (): Promise<void> => {
    try {
      const res = await setup(passwordFieldValue);
      if (res.status === 200) {
        //TODO routing
        return;
      }
      setErrorState(true);
    } catch (error) {
      setErrorState(true);
    }
  };

  return (
    <Card>
      <CardContent className="flex flex-col gap-4 items-center">
        <b className="text-5xl">FLAdmin</b>
        <span className="self-start">
          Enter the one-time setup password to continue.
        </span>
        <Input
          placeholder="Setup password"
          type="password"
          value={passwordFieldValue}
          onChange={(e) => setPasswordFieldValue(e.target.value)}
        />
        {errorState && (
          <span className="flex flex-row align-center gap-2 text-red-500">
            <InfoIcon />
            Something went wrong. Have you entered an incorrect password?
          </span>
        )}
        <Button
          className="w-full"
          disabled={!passwordFieldValue.length}
          onClick={onClickSubmit}
        >
          Submit
        </Button>
      </CardContent>
    </Card>
  );
}

export default SetupCard;
