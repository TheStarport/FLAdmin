import Editor from "@monaco-editor/react";
import {
  NavigationMenu,
  NavigationMenuList,
  NavigationMenuTrigger,
  NavigationMenuContent,
  NavigationMenuLink,
} from "../ui/navigation-menu";
import { navigationMenuTriggerStyle } from "../ui/navigation-menu-styles";
import { NavigationMenuItem } from "@radix-ui/react-navigation-menu";
import { useState } from "react";
import type ConfigFile from "@/types/config-file";
import { Tabs, TabsList, TabsTrigger } from "../ui/tabs";
import type { ReactNode } from "react";

function JsonEditor() {
  const [openConfigs, setOpenConfigs] = useState<ConfigFile[]>([]);
  const [value, setValue] = useState<string | undefined>();

  const listOpenFileTriggers: ReactNode[] = openConfigs.map(
    (openConfig: ConfigFile) => {
      return (
        <TabsTrigger value={openConfig.name}>{openConfig.name}</TabsTrigger>
      );
    }
  );

  return (
    <div className="h-screen w-full flex flex-col gap-2">
      <NavigationMenu>
        <NavigationMenuList>
          <NavigationMenuItem>
            <NavigationMenuTrigger>File</NavigationMenuTrigger>
            <NavigationMenuContent className="grid gap-4 min-w-max">
              <NavigationMenuLink>Create New Config</NavigationMenuLink>
              <NavigationMenuLink>Edit Existing Config</NavigationMenuLink>
              <NavigationMenuLink>Save Config</NavigationMenuLink>
              <NavigationMenuLink>Close Config</NavigationMenuLink>
            </NavigationMenuContent>
          </NavigationMenuItem>
          <NavigationMenuItem>
            <NavigationMenuLink className={navigationMenuTriggerStyle()}>
              Validate
            </NavigationMenuLink>
          </NavigationMenuItem>
        </NavigationMenuList>
      </NavigationMenu>
      {openConfigs.length > 0 && (
        <Tabs>
          <TabsList>{listOpenFileTriggers}</TabsList>
        </Tabs>
      )}
      {value ? (
        <Editor
          height="100%"
          defaultLanguage="json"
          value={value}
          theme="vs-dark"
          onChange={setValue}
          options={{
            minimap: { enabled: false },
            formatOnPaste: true,
            formatOnType: true,
          }}
        />
      ) : (
        <div className="w-full h-full flex justify-center items-center text-2xl text-muted-foreground bg-background">
          No Config Selected
        </div>
      )}
    </div>
  );
}

export default JsonEditor;
