export enum PlanType {
  Free = 1,
  Premium = 2,
}

export interface UserResponse {
  id: string;
  firstname: string;
  lastname: string;
  email: string;
  planType: PlanType;
  createdAt: string;
}

export interface UpdateUserRequest {
  firstname: string;
  lastname: string;
}

