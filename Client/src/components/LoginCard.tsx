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
import { login } from "@/services/fladmin";
import { InfoIcon, LoaderCircleIcon } from "lucide-react";

const loginFormSchema = z.object({
  username: z.string().min(1, "Username is required").trim(),
  password: z.string().min(1, "Username is required"),
});

function LoginCard() {
  const [errorState, setErrorState] = useState<boolean>(false);
  const [loadingState, setLoadingState] = useState<boolean>(false);

  const loginForm = useForm<z.infer<typeof loginFormSchema>>({
    resolver: zodResolver(loginFormSchema),
    mode: "onChange",
    defaultValues: {
      username: "",
      password: "",
    },
  });

  const onSubmit = async (values: z.infer<typeof loginFormSchema>) => {
    setLoadingState(true);
    try {
      const res = await login(values.username, values.password);
      if (res.status === 200) {
        //TODO routing
        setLoadingState(false);
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
      <CardHeader>
        <CardTitle className="flex flex-col gap-4 items-center">
          <b className="text-5xl">FLAdmin</b>
        </CardTitle>
      </CardHeader>
      <CardContent className="min-w-100">
        <Form {...loginForm}>
          <form
            onSubmit={loginForm.handleSubmit(onSubmit)}
            className="space-y-8"
          >
            <FormField
              control={loginForm.control}
              name="username"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Username</FormLabel>
                  <FormControl>
                    <Input placeholder="Your Username" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={loginForm.control}
              name="password"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Password</FormLabel>
                  <FormControl>
                    <Input
                      type="password"
                      placeholder="Your Password"
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
                  Something went wrong logging in. Did you enter an incorrect
                  password?
                </span>
              )}
              <Button
                className="flex-1"
                type="submit"
                disabled={!loginForm.formState.isValid || loadingState}
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

export default LoginCard;
