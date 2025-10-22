import CharacterActionsDropdown from "@/components/CharacterManagement/CharacterActionsDropdown";
import type { Character } from "@/types/character";
import type { Meta, StoryObj } from "@storybook/react-vite";
import { v4 as uuidv4 } from "uuid";

const meta = {
  title: "CharacterActionsDropdown",
  component: CharacterActionsDropdown,
  tags: ["autodocs"],
  argTypes: {},
  parameters: {
    docs: {
      description: {
        component:
          "Dropdown containing character actions: Rename, Kick, Kill, Delete and Move Account.",
      },
    },
  },
} satisfies Meta<typeof CharacterActionsDropdown>;

export default meta;
type Story = StoryObj<typeof meta>;

const exampleCharacter: Character = {
  characterName: "Test McTestington",
  accountId: uuidv4(),
  onlineStatus: true,
};

export const Default: Story = {
  args: {
    character: exampleCharacter,
  },
};
