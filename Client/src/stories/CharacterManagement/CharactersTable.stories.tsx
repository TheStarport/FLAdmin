import CharactersTable from "@/components/CharacterManagement/CharactersTable";
import type { Meta, StoryObj } from "@storybook/react-vite";
import type { Character } from "@/types/character";
import { charactersColumns } from "@/components/CharacterManagement/CharactersTableColumnDef";
import type { ColumnDef } from "@tanstack/react-table";
import { v4 as uuidv4 } from "uuid";

const meta = {
  title: "CharactersTable",
  component: CharactersTable,
  tags: ["autodocs"],
  argTypes: {
    data: {
      control: {
        type: "object",
      },
      description: "A list of Character types.",
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
          "Table to display character data, such as: name, corresponding account, online status, ...",
      },
    },
  },
} satisfies Meta<typeof CharactersTable>;

export default meta;
type Story = StoryObj<typeof meta>;

const exampleCharacters: Character[] = [
  { accountId: uuidv4(), characterName: "Thalindra", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Korin", onlineStatus: false },
  { accountId: uuidv4(), characterName: "Eryndor", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Lyssa", onlineStatus: false },
  { accountId: uuidv4(), characterName: "Varok", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Mirella", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Darius", onlineStatus: false },
  { accountId: uuidv4(), characterName: "Seraphine", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Kael", onlineStatus: false },
  { accountId: uuidv4(), characterName: "Riven", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Aldren", onlineStatus: false },
  { accountId: uuidv4(), characterName: "Sylara", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Toren", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Nyssa", onlineStatus: false },
  { accountId: uuidv4(), characterName: "Brennar", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Elara", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Gareth", onlineStatus: false },
  { accountId: uuidv4(), characterName: "Mirael", onlineStatus: false },
  { accountId: uuidv4(), characterName: "Jorvik", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Calista", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Fenric", onlineStatus: false },
  { accountId: uuidv4(), characterName: "Orin", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Selene", onlineStatus: false },
  { accountId: uuidv4(), characterName: "Ragnar", onlineStatus: true },
  { accountId: uuidv4(), characterName: "Arielle", onlineStatus: false },
];

export const Default: Story = {
  args: {
    columns: charactersColumns as ColumnDef<unknown, unknown>[],
    data: exampleCharacters,
  },
};
