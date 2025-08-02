import BanAccountDialog from "@/components/AccountManagement/BanAccountDialog";
import type { Meta, StoryObj } from "@storybook/react-vite";
import { http, HttpResponse } from "msw";

const meta = {
  title: "BanAccountDialog",
  component: BanAccountDialog,
  tags: ["autodocs"],
  argTypes: {},
  args: {
    banningAccountIds: ["account1"],
  },
  parameters: {
    docs: {
      description: {
        component:
          "Dialog to confirm ban of account(s) and for setting unban date.",
      },
    },
    msw: {
      handlers: [
        http.patch("/api/accounts/ban", () => {
          return HttpResponse.json({}, { status: 200 });
        }),
      ],
    },
  },
} satisfies Meta<typeof BanAccountDialog>;

export default meta;
type Story = StoryObj<typeof meta>;

export const Default: Story = {};

export const SuccessBanMany: Story = {
  args: {
    banningAccountIds: [
      "account1",
      "4d930265-7181-4ac8-97f7-65fbfe6c64d7",
      "account3",
      "account4",
      "7874d326-e9f9-4630-9b6e-f2a0da1b3f73",
      "3274cb36-4fc6-4dde-91aa-0db943cf672c",
    ],
  },
};

export const FailBanOne: Story = {
  parameters: {
    msw: {
      handlers: [
        http.patch("/api/accounts/ban", () => {
          return HttpResponse.json({}, { status: 500 });
        }),
      ],
    },
  },
};

export const FailBanSome: Story = {
  args: {
    banningAccountIds: [
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
        http.patch("/api/accounts/ban", ({ request }) => {
          const url = new URL(request.url);
          const accountId = url.searchParams.get("accountId");
          return accountId === "account1" ||
            accountId === "7874d326-e9f9-4630-9b6e-f2a0da1b3f73"
            ? HttpResponse.json({}, { status: 500 })
            : HttpResponse.json({}, { status: 200 });
        }),
      ],
    },
  },
};

export const FailBanMany: Story = {
  args: {
    banningAccountIds: [
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
        http.patch("/api/accounts/ban", () => {
          return HttpResponse.json({}, { status: 500 });
        }),
      ],
    },
  },
};

export const Timeout: Story = {
  parameters: {
    msw: {
      handlers: [
        http.patch("/api/accounts/ban", async () => {
          await new Promise(() => {});
        }),
      ],
    },
  },
};
