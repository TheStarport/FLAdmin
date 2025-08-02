import type { Meta, StoryObj } from "@storybook/react-vite";
import SetRolesGroup from "@/components/AccountManagement/SetRolesGroup";
import { fn } from "storybook/test";
import { useState } from "react";
import type { FLAdminRole } from "@/types/roles";

export const ActionsData = {
  onSetRole: fn(),
};

const meta = {
  title: "SetRolesGroup",
  component: SetRolesGroup,
  tags: ["autodocs"],
  excludeStories: /.*Data$/,
  argTypes: {
    accountRoles: {
      control: "object",
      description:
        "An array of strings, representing the roles which the account already has.",
    },
    validRoles: {
      control: "object",
      description:
        "An array of string, representing the roles which this account could potentially have.",
    },
    onSetRole: {
      action: "role toggled",
      description: "Callback function called when a role is toggled on or off.",
    },
  },
  args: {
    ...ActionsData,
  },
  parameters: {
    docs: {
      description: {
        component:
          "A dropdown and a card of badges, used to set the roles for an FLAdmin account.",
      },
    },
  },
} satisfies Meta<typeof SetRolesGroup>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  render: (args) => {
    const [accountRoles, setAccountRoles] = useState<FLAdminRole[]>(
      args.accountRoles!
    );
    const handleSetRole = (role: FLAdminRole) => {
      setAccountRoles((prev: FLAdminRole[]) =>
        prev.includes(role)
          ? prev.filter((r: FLAdminRole) => r !== role)
          : [...prev, role]
      );
      args.onSetRole!(role);
    };
    return (
      <SetRolesGroup
        {...args}
        accountRoles={accountRoles}
        onSetRole={handleSetRole}
      />
    );
  },
  args: {
    accountRoles: ["Web", "Game"] as FLAdminRole[],
    validRoles: [
      "Web",
      "Game",
      "ManageAdmins",
      "ManageAutomation",
      "ManageAccounts",
      "ManageServer",
      "SuperAdmin",
      "ManageRoles",
    ] as FLAdminRole[],
    onSetRole: ActionsData.onSetRole,
  },
};

export const ManyTags: Story = {
  args: {
    accountRoles: [
      "Web",
      "Game",
      "ManageAdmins",
      "ManageAutomation",
      "ManageAccounts",
      "ManageServer",
      "SuperAdmin",
      "ManageRoles",
    ] as FLAdminRole[],
    validRoles: ["Admin", "Player", "SuperAdmin"] as FLAdminRole[],
    onSetRole: ActionsData.onSetRole,
  },
};
