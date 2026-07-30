export interface RegisterRequest {
  firstname: string,
  lastname: string,
  email: string,
  password: string,
}

export interface LoginRequest {
  email: string,
  password: string,
}

export interface AuthResponse {
  accessToken: string,
}

export interface ConfirmEmailRequest {
  token: string,
}

export interface ResendConfirmationRequest {
  email: string,
}

export interface ForgotPasswordRequest {
  email: string,
}

export interface ResetPasswordRequest {
  token: string,
  newPassword: string,
}
