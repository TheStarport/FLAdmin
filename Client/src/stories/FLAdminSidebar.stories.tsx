import type { Meta, StoryObj } from "@storybook/react-vite";
import FLAdminSidebar from "@/components/FLAdminSidebar";
import { SidebarProvider } from "@/components/ui/sidebar";

const meta = {
  title: "FLAdminSidebar",
  component: FLAdminSidebar,
  tags: ["autodocs"],
  argTypes: {},
  args: {},
  parameters: {
    docs: {
      description: {
        component:
          "Sidebar displayed on the left side of the screen, contains links to different pages.",
      },
    },
  },
} satisfies Meta<typeof FLAdminSidebar>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  render: () => (
    <SidebarProvider>
      <FLAdminSidebar />
    </SidebarProvider>
  ),
};
