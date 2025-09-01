import type { Meta, StoryObj } from "@storybook/react-vite";
import LoginCard from "@/components/LoginCard";
import { http, HttpResponse } from "msw";

const meta = {
  title: "LoginCard",
  component: LoginCard,
  tags: ["autodocs"],
  argTypes: {},
  args: {},
  parameters: {
    docs: {
      description: {
        component:
          "Card displayed on login page, contains Username and Password entry fields.",
      },
    },
  },
} satisfies Meta<typeof LoginCard>;

export default meta;
type Story = StoryObj<typeof meta>;

export const LoginSuccess: Story = {
  parameters: {
    msw: {
      handlers: [
        http.post("/api/auth/login", () => {
          return HttpResponse.json({}, { status: 200 });
        }),
      ],
    },
  },
};

export const LoginError: Story = {
  parameters: {
    msw: {
      handlers: [
        http.post("/api/auth/login", () => {
          return HttpResponse.json({}, { status: 500 });
        }),
      ],
    },
  },
};
