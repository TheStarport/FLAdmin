import type { Meta, StoryObj } from "@storybook/react-vite";
import SetRolesGroup from "@/components/AccountManagement/SetRolesGroup";
import { fn } from "storybook/test";
import { useState } from "react";

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
    const [accountRoles, setAccountRoles] = useState<string[]>(args.accountRoles!);
    const handleSetRole = (role: string) => {
      setAccountRoles((prev: string[]) => 
        prev.includes(role) 
          ? prev.filter((r: string) => r !== role)
          : [...prev, role]
      );
      args.onSetRole!(role);
    };
    return <SetRolesGroup {...args} accountRoles={accountRoles} onSetRole={handleSetRole} />;
  },
  args: {
    accountRoles: ["Player"],
    validRoles: ["Admin", "Player", "Superadmin"],
    onSetRole: ActionsData.onSetRole,
  },
};

export const ManyTags: Story = {
  args: {
    accountRoles: [
      "Player",
      "Role_1",
      "Role_2",
      "Role_3",
      "Role_4",
      "Role_5",
      "Role_6",
      "Role_7",
      "Role_8",
      "Role_9",
      "Role_10",
      "Role_11",
      "Role_12",
      "Role_13",
      "Role_14",
      "Role_15",
      "Role_16",
      "Role_17",
      "Role_18",
      "Role_19",
      "Role_20",
      "Role_21",
      "Role_22",
      "Role_23",
      "Role_24",
      "Role_25",
      "Role_26",
    ],
    validRoles: ["Admin", "Player", "Superadmin"],
    onSetRole: ActionsData.onSetRole,
  },
};
