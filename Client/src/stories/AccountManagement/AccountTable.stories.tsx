import AccountTable from "@/components/AccountManagement/AccountTable";
import { accountColumns } from "@/components/AccountManagement/AccountTableColumnDef";
import type Account from "@/types/account";
import type { Meta, StoryObj } from "@storybook/react-vite";
import { v4 as uuidv4 } from "uuid";

import type { ColumnDef } from "@tanstack/react-table";

const meta = {
  title: "AccountTable",
  component: AccountTable,
  tags: ["autodocs"],
  argTypes: {
    data: {
      control: {
        type: "object",
      },
      description: "A list of Account types.",
    },
    columns: {
      control: {
        type: "object",
      },
    },
  },
  parameters: {
    docs: {
      description: {
        component:
          "Table to display account data, such as: Ban status & unban date, online status, roles and cash.",
      },
    },
  },
} satisfies Meta<typeof AccountTable>;

export default meta;
type Story = StoryObj<typeof meta>;

const exampleAccounts: Account[] = [
  {
    id: uuidv4(),
    cash: 150000,
    lastOnline: "2024-01-15T10:30:00Z",
    gameRoles: ["Game"],
    webRoles: ["Web"],
  },
  {
    id: uuidv4(),
    cash: 2500000,
    lastOnline: "2024-01-14T15:45:00Z",
    gameRoles: ["Game", "ManageAccounts"],
    webRoles: ["Web"],
  },
  {
    id: uuidv4(),
    cash: 75000,
    scheduledUnbanDate: "2024-02-01T00:00:00Z",
    gameRoles: ["Game"],
  },
  {
    id: uuidv4(),
    cash: 890000,
    lastOnline: "2024-01-16T08:20:00Z",
    gameRoles: ["Game"],
    webRoles: ["Web", "ManageServer"],
  },
  {
    id: uuidv4(),
    cash: 45000,
    lastOnline: "2024-01-13T22:15:00Z",
    gameRoles: ["Game"],
  },
  {
    id: uuidv4(),
    cash: 5000000,
    lastOnline: "2024-01-16T12:00:00Z",
    gameRoles: ["SuperAdmin", "Game"],
    webRoles: ["Web"],
  },
  {
    id: uuidv4(),
    cash: 320000,
    scheduledUnbanDate: "2024-01-25T00:00:00Z",
  },
  {
    id: uuidv4(),
    cash: 180000,
    lastOnline: "2024-01-15T19:30:00Z",
    gameRoles: ["Game", "ManageAutomation"],
    webRoles: ["Web"],
  },
  {
    id: uuidv4(),
    cash: 95000,
    lastOnline: "2024-01-12T14:45:00Z",
    gameRoles: ["Game"],
  },
  {
    id: uuidv4(),
    cash: 1200000,
    lastOnline: "2024-01-16T09:15:00Z",
    gameRoles: ["Game"],
    webRoles: ["Web", "ManageAdmins"],
  },
  {
    id: uuidv4(),
    cash: 67000,
    scheduledUnbanDate: "2024-03-15T00:00:00Z",
    gameRoles: ["Game"],
  },
  {
    id: uuidv4(),
    cash: 450000,
    lastOnline: "2024-01-16T11:20:00Z",
    gameRoles: ["Game"],
    webRoles: ["Web", "Database"],
  },
  {
    id: uuidv4(),
    cash: 230000,
    lastOnline: "2024-01-14T16:30:00Z",
    gameRoles: ["Game"],
  },
  {
    id: uuidv4(),
    cash: 3200000,
    lastOnline: "2024-01-16T13:45:00Z",
    gameRoles: ["Game"],
    webRoles: ["Web", "ManageRoles"],
  },
  {
    id: uuidv4(),
    cash: 89000,
    scheduledUnbanDate: "2024-02-10T00:00:00Z",
  },
  {
    id: uuidv4(),
    cash: 560000,
    lastOnline: "2024-01-15T20:10:00Z",
    gameRoles: ["Game"],
    webRoles: ["Web"],
  },
  {
    id: uuidv4(),
    cash: 125000,
    lastOnline: "2024-01-11T18:25:00Z",
    gameRoles: ["Game"],
  },
  {
    id: uuidv4(),
    cash: 780000,
    lastOnline: "2024-01-16T07:50:00Z",
    gameRoles: ["Game", "ManageServer"],
    webRoles: ["Web"],
  },
  {
    id: uuidv4(),
    cash: 34000,
    scheduledUnbanDate: "2024-01-30T00:00:00Z",
    gameRoles: ["Game"],
  },
  {
    id: uuidv4(),
    cash: 1500000,
    lastOnline: "2024-01-16T14:15:00Z",
    gameRoles: ["Game", "ManageAccounts", "ManageAutomation"],
    webRoles: ["Web"],
  },
];

export const Default: Story = {
  args: {
    columns: accountColumns as ColumnDef<unknown, unknown>[],
    data: exampleAccounts,
  },
};
