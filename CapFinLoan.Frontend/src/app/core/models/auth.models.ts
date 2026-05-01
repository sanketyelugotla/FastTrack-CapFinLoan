export interface LoginRequest {
  email: string;
  password: string;
}

export interface SignupRequest {
  name: string;
  email: string;
  phone: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  role: string;
  userId: string;
  name: string;
  email: string;
}

export interface OtpVerificationRequest {
  email: string;
  otpCode: string;
  name: string;
  phone: string;
  password: string;
}

export interface OtpSendResponse {
  success: boolean;
  message: string;
  email: string;
  expiryMinutes: number;
}

export interface ResetPasswordWithOtpRequest {
  email: string;
  otpCode: string;
  newPassword: string;
}

export interface PasswordResetResponse {
  success: boolean;
  message: string;
  email: string;
}

export interface UserSummary {
  id: string;
  name: string;
  email: string;
  phone: string;
  role: string;
  isActive: boolean;
  createdAtUtc: string;
}
