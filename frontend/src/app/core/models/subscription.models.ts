import { PlanType } from './user.models'

export interface SubscriptionResponse {
  plan: PlanType;
  premiumUntil: string | null;
  isActive: boolean;
}

export interface CheckoutSessionResponse {
  url: string;
}
