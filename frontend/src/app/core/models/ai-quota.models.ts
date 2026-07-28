import { PlanType } from './user.models';

export interface AiQuotaStatus {
  used: number;
  limit: number;
  resetAt: string;
  plan: PlanType;
}
