import ConfirmationDialog from "@/components/AccountManagement/ConfirmationDialog";
import type { Meta, StoryObj } from "@storybook/react-vite";
import { http, HttpResponse } from "msw";

const meta = {
  title: "ConfirmationDialog",
  component: ConfirmationDialog,
  tags: ["autodocs"],
  argTypes: {
    variant: {
      control: "text",
      options: ["Unban Account", "Delete Account", "Remove Characters"],
      description:
        "What type of confirmation dialog to open, corresponding to the action being confirmed.",
    },
  },
  args: {
    variant: "Unban Account",
  },
  parameters: {
    docs: {
      description: {
        component:
          "Dialog to confirm an account action, such as unban, account deletion and removal of all characters.",
      },
    },
    msw: {
      handlers: [
        http.patch("/api/accounts/unban", () => {
          return HttpResponse.json({}, { status: 200 });
        }),
        http.delete("/api/accounts/delete", () => {
          return HttpResponse.json({}, { status: 200 });
        }),
        http.delete("/api/characters/removeallfromaccount", () => {
          return HttpResponse.json({}, { status: 200 });
        }),
      ],
    },
  },
} satisfies Meta<typeof ConfirmationDialog>;

export default meta;
type Story = StoryObj<typeof meta>;

// Unban Stories
export const UnbanSuccessOne: Story = {
  args: {
    variant: "Unban Account",
    editingAccountIds: ["account1"],
  },
};

export const UnbanSuccessMany: Story = {
  args: {
    variant: "Unban Account",
    editingAccountIds: [
      "account1",
      "4d930265-7181-4ac8-97f7-65fbfe6c64d7",
      "account3",
      "account4",
      "7874d326-e9f9-4630-9b6e-f2a0da1b3f73",
      "3274cb36-4fc6-4dde-91aa-0db943cf672c",
    ],
  },
};

export const UnbanFailFew: Story = {
  args: {
    variant: "Unban Account",
    editingAccountIds: [
      "account1",
      "4d930265-7181-4ac8-97f7-65fbfe6c64d7",
      "account3",
      "account4",
      "7874d326-e9f9-4630-9b6e-f2a0da1b3f73",
      "3274cb36-4fc6-4dde-91aa-0db943cf672c",
    ],
  },
  parameters: {
    msw: {
      handlers: [
        http.patch("/api/accounts/unban", async ({ request }) => {
          const accountId = JSON.parse(await request.text()).data;
          return accountId === "account1" ||
            accountId === "7874d326-e9f9-4630-9b6e-f2a0da1b3f73"
            ? HttpResponse.json({}, { status: 500 })
            : HttpResponse.json({}, { status: 200 });
        }),
      ],
    },
  },
};

export const UnbanFailMany: Story = {
  args: {
    variant: "Unban Account",
    editingAccountIds: [
      "account1",
      "4d930265-7181-4ac8-97f7-65fbfe6c64d7",
      "account3",
      "account4",
      "7874d326-e9f9-4630-9b6e-f2a0da1b3f73",
      "3274cb36-4fc6-4dde-91aa-0db943cf672c",
    ],
  },
  parameters: {
    msw: {
      handlers: [
        http.patch("/api/accounts/unban", () => {
          return HttpResponse.json({}, { status: 500 });
        }),
      ],
    },
  },
};

// Delete Stories
export const DeleteSuccessOne: Story = {
  args: {
    variant: "Delete Account",
    editingAccountIds: ["account1"],
  },
};

export const DeleteSuccessMany: Story = {
  args: {
    variant: "Delete Account",
    editingAccountIds: [
      "account1",
      "4d930265-7181-4ac8-97f7-65fbfe6c64d7",
      "account3",
      "account4",
      "7874d326-e9f9-4630-9b6e-f2a0da1b3f73",
      "3274cb36-4fc6-4dde-91aa-0db943cf672c",
    ],
  },
};

export const DeleteFailMany: Story = {
  args: {
    variant: "Delete Account",
    editingAccountIds: [
      "account1",
      "4d930265-7181-4ac8-97f7-65fbfe6c64d7",
      "account3",
      "account4",
      "7874d326-e9f9-4630-9b6e-f2a0da1b3f73",
      "3274cb36-4fc6-4dde-91aa-0db943cf672c",
    ],
  },
  parameters: {
    msw: {
      handlers: [
        http.delete("/api/accounts/delete", () => {
          return HttpResponse.json({}, { status: 500 });
        }),
      ],
    },
  },
};

// Remove Characters Stories
export const RemoveCharactersSuccessOne: Story = {
  args: {
    variant: "Remove Characters",
    editingAccountIds: ["account1"],
  },
};

export const RemoveCharactersSuccessMany: Story = {
  args: {
    variant: "Remove Characters",
    editingAccountIds: [
      "account1",
      "4d930265-7181-4ac8-97f7-65fbfe6c64d7",
      "account3",
      "account4",
      "7874d326-e9f9-4630-9b6e-f2a0da1b3f73",
      "3274cb36-4fc6-4dde-91aa-0db943cf672c",
    ],
  },
};

export const RemoveCharactersFailFew: Story = {
  args: {
    variant: "Remove Characters",
    editingAccountIds: [
      "account1",
      "4d930265-7181-4ac8-97f7-65fbfe6c64d7",
      "account3",
      "account4",
      "7874d326-e9f9-4630-9b6e-f2a0da1b3f73",
      "3274cb36-4fc6-4dde-91aa-0db943cf672c",
    ],
  },
  parameters: {
    msw: {
      handlers: [
        http.delete(
          "/api/characters/removeallfromaccount",
          async ({ request }) => {
            const accountText = await request.text();
            const accountId = accountText.slice(1, accountText.length - 1);
            console.log("ACCOUNT ID: " + accountId);
            return accountId === "account1" ||
              accountId === "7874d326-e9f9-4630-9b6e-f2a0da1b3f73"
              ? HttpResponse.json({}, { status: 500 })
              : HttpResponse.json({}, { status: 200 });
          }
        ),
      ],
    },
  },
};

export const RemoveCharactersFailMany: Story = {
  args: {
    variant: "Remove Characters",
    editingAccountIds: [
      "account1",
      "4d930265-7181-4ac8-97f7-65fbfe6c64d7",
      "account3",
      "account4",
      "7874d326-e9f9-4630-9b6e-f2a0da1b3f73",
      "3274cb36-4fc6-4dde-91aa-0db943cf672c",
    ],
  },
  parameters: {
    msw: {
      handlers: [
        http.delete("/api/characters/removeallfromaccount", () => {
          return HttpResponse.json({}, { status: 500 });
        }),
      ],
    },
  },
};

export const RemoveCharactersTimeout: Story = {
  args: {
    variant: "Remove Characters",
    editingAccountIds: ["account1"],
  },
  parameters: {
    msw: {
      handlers: [
        http.delete("/api/characters/removeallfromaccount", async () => {
          await new Promise(() => {});
        }),
      ],
    },
  },
};
