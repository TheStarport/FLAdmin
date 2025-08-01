import type { Meta, StoryObj } from "@storybook/react-vite";
import AccountManagementDialog from "@/components/AccountManagement/AccountManagementDialog";

const meta = {
  title: "AccountManagementDialog",
  component: AccountManagementDialog,
  parameters: {},
  tags: ["autodocs"],
  argTypes: {},
  args: {},
} satisfies Meta<typeof AccountManagementDialog>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {};
