import RenameCharacterDialog from "@/components/CharacterManagement/RenameCharacterDialog";
import type { Meta, StoryObj } from "@storybook/react-vite";
import { http, HttpResponse } from "msw";

const meta = {
  title: "RenameCharacterDialog",
  component: RenameCharacterDialog,
  tags: ["autodocs"],
  argTypes: {
    oldCharacterName: {
      control: "text",
      description: "The name character's old name.",
    },
  },
  parameters: {
    docs: {
      description: {
        component: "Dialog allowing user to rename character.",
      },
    },
  },
} satisfies Meta<typeof RenameCharacterDialog>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {
  render: () => (
    <RenameCharacterDialog
      dialogOpen={true}
      setDialogOpen={() => {}}
      oldCharacterName="Test McTestington"
    />
  ),
  args: {
    dialogOpen: true,
    setDialogOpen: () => {},
    oldCharacterName: "Test McTestington",
  },
  parameters: {
    msw: {
      handlers: [
        http.patch("/api/characters/rename", () => {
          return HttpResponse.json({}, { status: 200 });
        }),
      ],
    },
  },
};

export const Error: Story = {
  render: () => (
    <RenameCharacterDialog
      dialogOpen={true}
      setDialogOpen={() => {}}
      oldCharacterName="Test McTestington"
    />
  ),
  args: {
    dialogOpen: true,
    setDialogOpen: () => {},
    oldCharacterName: "Test McTestington",
  },
  parameters: {
    msw: {
      handlers: [
        http.patch("/api/characters/rename", () => {
          return HttpResponse.json({}, { status: 500 });
        }),
      ],
    },
  },
};
