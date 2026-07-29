import { PlanType } from './user.models';

export enum UserRole {
  User = 1,
  Admin = 2,
}

export interface AdminUserListItem {
  id: string;
  email: string;
  firstname: string;
  lastname: string;
  role: UserRole;
  plan: PlanType;
  premiumUntil: string | null;
  isBanned: boolean;
  createdAt: string;
}

export interface AdminUserDetail extends AdminUserListItem {
  aiUsageCount: number;
  aiUsageResetAt: string;
  stripeCustomerId: string | null;
  stripeSubscriptionId: string | null;
}

export interface AdminUsersPage {
  items: AdminUserListItem[];
  total: number;
  page: number;
  size: number;
}

export interface BannedIp {
  id: string;
  ipAddress: string;
  reason: string | null;
  bannedByAdminId: string;
  createdAt: string;
}

export interface AdminStatsResponse {
  totalUsers: number;
  freeUsers: number;
  premiumUsers: number;
  bannedUsers: number;
  admins: number;
  totalCandidatures: number;
  totalAiAnalysesThisMonth: number;
  signupsLast30Days: number;
  signupsLast7Days: number;
}

export interface AdminAuditLog {
  id: string;
  adminId: string;
  targetUserId: string | null;
  action: string;
  details: string | null;
  ipAddress: string | null;
  createdAt: string;
}

export interface LogEntry {
  rawLine: string;
  level: string | null;
  timestamp: string | null;
}

export interface LogsResponse {
  entries: LogEntry[];
  fileName: string;
  totalLinesInFile: number;
}
