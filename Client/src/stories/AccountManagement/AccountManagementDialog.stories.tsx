import type { Meta, StoryObj } from "@storybook/react-vite";
import AccountManagementDialog from "@/components/AccountManagement/AccountManagementDialog";
import { http, HttpResponse } from "msw";

const meta = {
  title: "AccountManagementDialog",
  component: AccountManagementDialog,
  tags: ["autodocs"],
  argTypes: {
    editingAccountId: {
      control: "text",
      description:
        "The ID of the account to edit. Needed for API calls to fetch account data.",
    },
  },
  args: {},
  parameters: {
    docs: {
      description: {
        component: "Dialog to edit account data, as well as delete / ban.",
      },
    },
  },
} satisfies Meta<typeof AccountManagementDialog>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  parameters: {
    msw: {
      handlers: [
        http.get("/api/accounts/123", () => {
          return HttpResponse.json({
            id: "123",
            gameRoles: ["Game", "ManageAccounts"],
            webRoles: ["Web"],
            cash: 50000,
            lastOnline: "2024-01-15T10:30:00Z",
          });
        }),
      ],
    },
  },
  args: {
    editingAccountId: "123",
  },
};

export const Loading: Story = {
  parameters: {
    msw: {
      handlers: [
        http.get("/api/accounts/456", () => {
          return new Promise(() => {}); // Never resolves to simulate loading
        }),
      ],
    },
  },
  args: {
    editingAccountId: "456",
  },
};

export const Error: Story = {
  parameters: {
    msw: {
      handlers: [
        http.get("/api/accounts/789", () => {
          return HttpResponse.json(
            { error: "Account not found" },
            { status: 404 }
          );
        }),
      ],
    },
  },
  args: {
    editingAccountId: "789",
  },
};
