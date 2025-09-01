import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuItem,
  SidebarMenuButton,
  SidebarHeader,
  SidebarFooter,
} from "./ui/sidebar";
import {
  MonitorIcon,
  PuzzleIcon,
  ShieldUserIcon,
  UserIcon,
  User2Icon,
  UsersIcon,
  FileCogIcon,
  LogOutIcon,
} from "lucide-react";

function FLAdminSidebar() {
  const sidebarItems = [
    {
      name: "Server Dashboard",
      link: "/dashboard",
      icon: MonitorIcon,
    },
    {
      name: "Plugin Manager",
      link: "/plugins",
      icon: PuzzleIcon,
    },
    {
      name: "Admin Manager",
      link: "/admin",
      icon: ShieldUserIcon,
    },
    {
      name: "Accounts",
      link: "/accounts",
      icon: UserIcon,
    },
    {
      name: "Characters",
      link: "/characters",
      icon: UsersIcon,
    },
    {
      name: "Scripts",
      link: "/scripts",
      icon: FileCogIcon,
    },
  ];

  return (
    <Sidebar>
      <SidebarHeader>
        <b className="text-4xl">FLAdmin</b>
      </SidebarHeader>
      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel>Application Pages</SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {sidebarItems.map((item) => (
                <SidebarMenuItem key={item.name}>
                  <SidebarMenuButton asChild>
                    <a href={item.link}>
                      <item.icon />
                      <span>{item.name}</span>
                    </a>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>
      <SidebarFooter>
        <SidebarMenuButton>
          {/* TODO: Once user management is implemented, update */}
          <User2Icon /> Username
          <LogOutIcon className="ml-auto" />
        </SidebarMenuButton>
      </SidebarFooter>
    </Sidebar>
  );
}

export default FLAdminSidebar;
