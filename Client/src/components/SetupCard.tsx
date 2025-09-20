import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import {
  Form,
  FormField,
  FormControl,
  FormLabel,
  FormItem,
  FormMessage,
} from "./ui/form";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { useState } from "react";
import { setup } from "@/services/fladmin";
import { InfoIcon, LoaderCircleIcon } from "lucide-react";

const setupFormSchema = z.object({
  password: z.string().min(1, "Setup password is required"),
});

function SetupCard() {
  const [errorState, setErrorState] = useState<boolean>(false);
  const [loadingState, setLoadingState] = useState<boolean>(false);

  const setupForm = useForm<z.infer<typeof setupFormSchema>>({
    resolver: zodResolver(setupFormSchema),
    mode: "onChange",
    defaultValues: {
      password: "",
    },
  });

  const onSubmit = async (values: z.infer<typeof setupFormSchema>) => {
    setLoadingState(true);
    try {
      const res = await setup(values.password);
      if (res.status === 200) {
        setLoadingState(false);
        //TODO routing
        return;
      }
      setErrorState(true);
    } catch {
      setErrorState(true);
    }
    setLoadingState(false);
  };

  return (
    <Card>
      <CardContent>
        <CardHeader>
          <CardTitle className="flex flex-col gap-4 items-center">
            <b className="text-5xl">FLAdmin</b>
          </CardTitle>
        </CardHeader>
        <Form {...setupForm}>
          <form
            onSubmit={setupForm.handleSubmit(onSubmit)}
            className="space-y-8"
          >
            <FormField
              control={setupForm.control}
              name="password"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>One-Time Setup Password</FormLabel>
                  <FormControl>
                    <Input
                      type="password"
                      placeholder="Setup password"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <div className="flex flex-col gap-2">
              {errorState && (
                <span className="flex flex-row gap-2 text-red-500">
                  <InfoIcon />
                  Something went wrong. Have you entered an incorrect password?
                </span>
              )}
              <Button
                className="flex-1"
                type="submit"
                disabled={!setupForm.formState.isValid || loadingState}
              >
                {loadingState && <LoaderCircleIcon className="animate-spin" />}
                Submit
              </Button>
            </div>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}

export default SetupCard;
