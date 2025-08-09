import type { Meta, StoryObj } from "@storybook/react-vite";
import SetupCard from "@/components/SetupCard";
import { http, HttpResponse } from "msw";

const meta = {
  title: "SetupCard",
  component: SetupCard,
  tags: ["autodocs"],
  argTypes: {},
  args: {},
  parameters: {
    docs: {
      description: {
        component:
          "Card displayed on first-time setup, where one-time setup password is typed in.",
      },
    },
  },
} satisfies Meta<typeof SetupCard>;

export default meta;
type Story = StoryObj<typeof meta>;

export const SetupSuccess: Story = {
  parameters: {
    msw: {
      handlers: [
        http.post("/api/auth/setup", () => {
          return HttpResponse.json({}, { status: 200 });
        }),
      ],
    },
  },
};

export const SetupError: Story = {
  parameters: {
    msw: {
      handlers: [
        http.post("/api/auth/setup", () => {
          return HttpResponse.json({}, { status: 500 });
        }),
      ],
    },
  },
};
