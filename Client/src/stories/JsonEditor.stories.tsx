import type { Meta, StoryObj } from "@storybook/react-vite";
import JsonEditor from "@/components/PluginManagement/JsonEditor";

const meta = {
  title: "JsonEditor",
  component: JsonEditor,
  tags: ["autodocs"],
  argTypes: {
    value: {
      control: "text",
      description: "The JSON value to be edited.",
    },
    onChange: {
      description: "Callback function to handle changes in the JSON value.",
    },
  },
  args: {},
  parameters: {
    docs: {
      description: {
        component: "JsonEditor component for editing JSON data.",
      },
    },
  },
} satisfies Meta<typeof JsonEditor>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {};
