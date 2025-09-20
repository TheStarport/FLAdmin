import type { ObjectId } from "bson";
import type { FLAdminRole } from "./roles";

export default interface Account {
  id: string;
  characters?: ObjectId[];
  scheduledUnbanDate?: string;
  lastOnline?: string;
  gameRoles?: FLAdminRole[];
  webRoles?: FLAdminRole[];
  cash?: number;
}
