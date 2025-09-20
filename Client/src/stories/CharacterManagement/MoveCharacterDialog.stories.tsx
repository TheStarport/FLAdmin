import MoveCharacterDialog from "@/components/CharacterManagement/MoveCharacterDialog";
import type { Meta, StoryObj } from "@storybook/react-vite";
import { http, HttpResponse } from "msw";

const meta = {
  title: "MoveCharacterDialog",
  component: MoveCharacterDialog,
  tags: ["autodocs"],
  argTypes: {
    characterIds: {
      control: "object",
      description:
        "An array of IDs or character names which will be moved to the new target account.",
    },
    accountIds: {
      control: "object",
      description: "An array of account IDs to display in the search widget.",
    },
    pageSize: {
      control: "number",
      description:
        "Size of pagination for widget displaying selectable accounts.",
    },
  },
  args: {},
  parameters: {
    docs: {
      description: {
        component:
          "Dialog to display accounts and move characters to a new target account.",
      },
    },
  },
} satisfies Meta<typeof MoveCharacterDialog>;

export default meta;
type Story = StoryObj<typeof meta>;

export const SuccessMove: Story = {
  args: {
    characterIds: ["a0c143c8-9f43-4285-b17e-78d73961d892"],
    pageSize: 6,
    accountIds: [
      "a6e9c1b3-9de1-48f7-8e17-c8f9e4c27c59",
      "4a3e9ec1-92a1-4fd8-bc43-6e55e823a276",
      "9fc11013-5135-4390-9f52-9b91bc8193a1",
      "2fdb7d9a-8d91-41a4-a0d1-8c3bc87260dc",
      "af054ed6-49c7-4c95-9e64-984b2a3a1d9f",
      "3fd0b802-b3b7-496d-9a59-1f6941ad82e4",
      "0fae633d-6f0f-41d2-9231-3831fc49c7cb",
      "de1a60f9-7c4d-4c9d-a2b7-4044b3d203d0",
      "46aeab90-12c0-4057-9dc0-2d4602f5e6b6",
      "ce03e5e4-9119-4d5b-abe0-3f1327d97c4d",
      "6d78e569-c7a2-49ac-a154-f6e198ec24eb",
      "b21b9346-1b15-4df8-88ff-3dc0a6de0a9c",
      "f1761d63-24b5-4e3b-a3d3-e2231fd2eacc",
      "f2baf13d-30f4-40f6-a9ee-6ff48b3dc06f",
      "e1760cc1-4f9f-4bcd-8fa7-fb6fc8030c27",
      "0f1f86cb-cc1d-4a3c-a53b-5e53d5e2b23a",
      "16873ed2-29e3-45c8-a59f-112a6a37de1a",
      "abb3b30b-e355-42a9-9bb2-c0dfc69337a2",
      "8b7b3b30-bb8e-4d3d-b5d2-91a18963f013",
      "287fb54d-f7e6-4e5b-bfad-9d2c5b0bc58e",
      "59c9b394-9012-4b2e-bfd3-56027b08a057",
      "964af930-15e3-4c55-8b70-008da38dbbbb",
      "8f8d0072-236b-42f5-b3b4-6b7c2dc5f1df",
      "4b0a7400-1d49-41b2-b88c-02c6208b0a3c",
      "d4f87ef6-33fa-4c95-a76c-df94dfdb8196",
    ],
  },
  parameters: {
    msw: {
      handlers: [
        http.patch("/api/characters/movecharactertoaccount", () => {
          return HttpResponse.json({}, { status: 200 });
        }),
      ],
    },
  },
};

export const FailMoveFew: Story = {
  args: {
    characterIds: [
      "a0c143c8-9f43-4285-b17e-78d73961d892",
      "Character McDude",
      "b48e989e-7968-4a24-9f68-06d78103ea8c",
    ],
    pageSize: 6,
    accountIds: [
      "a6e9c1b3-9de1-48f7-8e17-c8f9e4c27c59",
      "4a3e9ec1-92a1-4fd8-bc43-6e55e823a276",
      "9fc11013-5135-4390-9f52-9b91bc8193a1",
      "2fdb7d9a-8d91-41a4-a0d1-8c3bc87260dc",
      "af054ed6-49c7-4c95-9e64-984b2a3a1d9f",
      "3fd0b802-b3b7-496d-9a59-1f6941ad82e4",
      "0fae633d-6f0f-41d2-9231-3831fc49c7cb",
      "de1a60f9-7c4d-4c9d-a2b7-4044b3d203d0",
      "46aeab90-12c0-4057-9dc0-2d4602f5e6b6",
      "ce03e5e4-9119-4d5b-abe0-3f1327d97c4d",
      "6d78e569-c7a2-49ac-a154-f6e198ec24eb",
      "b21b9346-1b15-4df8-88ff-3dc0a6de0a9c",
      "f1761d63-24b5-4e3b-a3d3-e2231fd2eacc",
      "f2baf13d-30f4-40f6-a9ee-6ff48b3dc06f",
      "e1760cc1-4f9f-4bcd-8fa7-fb6fc8030c27",
      "0f1f86cb-cc1d-4a3c-a53b-5e53d5e2b23a",
      "16873ed2-29e3-45c8-a59f-112a6a37de1a",
      "abb3b30b-e355-42a9-9bb2-c0dfc69337a2",
      "8b7b3b30-bb8e-4d3d-b5d2-91a18963f013",
      "287fb54d-f7e6-4e5b-bfad-9d2c5b0bc58e",
      "59c9b394-9012-4b2e-bfd3-56027b08a057",
      "964af930-15e3-4c55-8b70-008da38dbbbb",
      "8f8d0072-236b-42f5-b3b4-6b7c2dc5f1df",
      "4b0a7400-1d49-41b2-b88c-02c6208b0a3c",
      "d4f87ef6-33fa-4c95-a76c-df94dfdb8196",
    ],
  },
  parameters: {
    msw: {
      handlers: [
        http.patch("/api/characters/movecharactertoaccount", () => {
          return HttpResponse.json({}, { status: 500 });
        }),
      ],
    },
  },
};

export const FailMoveMany: Story = {
  args: {
    characterIds: [
      "a0c143c8-9f43-4285-b17e-78d73961d892",
      "Character McDude",
      "b48e989e-7968-4a24-9f68-06d78103ea8c",
      "Dude O'Man",
      "61216f13-b761-49b5-98df-1cd51d5bb3b4",
      "e746a3ef-61dd-4e36-a0f1-aa9926f0d748",
    ],
    pageSize: 6,
    accountIds: [
      "a6e9c1b3-9de1-48f7-8e17-c8f9e4c27c59",
      "4a3e9ec1-92a1-4fd8-bc43-6e55e823a276",
      "9fc11013-5135-4390-9f52-9b91bc8193a1",
      "2fdb7d9a-8d91-41a4-a0d1-8c3bc87260dc",
      "af054ed6-49c7-4c95-9e64-984b2a3a1d9f",
      "3fd0b802-b3b7-496d-9a59-1f6941ad82e4",
      "0fae633d-6f0f-41d2-9231-3831fc49c7cb",
      "de1a60f9-7c4d-4c9d-a2b7-4044b3d203d0",
      "46aeab90-12c0-4057-9dc0-2d4602f5e6b6",
      "ce03e5e4-9119-4d5b-abe0-3f1327d97c4d",
      "6d78e569-c7a2-49ac-a154-f6e198ec24eb",
      "b21b9346-1b15-4df8-88ff-3dc0a6de0a9c",
      "f1761d63-24b5-4e3b-a3d3-e2231fd2eacc",
      "f2baf13d-30f4-40f6-a9ee-6ff48b3dc06f",
      "e1760cc1-4f9f-4bcd-8fa7-fb6fc8030c27",
      "0f1f86cb-cc1d-4a3c-a53b-5e53d5e2b23a",
      "16873ed2-29e3-45c8-a59f-112a6a37de1a",
      "abb3b30b-e355-42a9-9bb2-c0dfc69337a2",
      "8b7b3b30-bb8e-4d3d-b5d2-91a18963f013",
      "287fb54d-f7e6-4e5b-bfad-9d2c5b0bc58e",
      "59c9b394-9012-4b2e-bfd3-56027b08a057",
      "964af930-15e3-4c55-8b70-008da38dbbbb",
      "8f8d0072-236b-42f5-b3b4-6b7c2dc5f1df",
      "4b0a7400-1d49-41b2-b88c-02c6208b0a3c",
      "d4f87ef6-33fa-4c95-a76c-df94dfdb8196",
    ],
  },
  parameters: {
    msw: {
      handlers: [
        http.patch("/api/characters/movecharactertoaccount", () => {
          return HttpResponse.json({}, { status: 500 });
        }),
      ],
    },
  },
};
