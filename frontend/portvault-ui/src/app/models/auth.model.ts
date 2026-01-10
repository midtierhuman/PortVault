export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email?: string;
  username?: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresUtc: string;
  username: string;
  email: string;
  role: Role;
}

export interface AuthUser {
  id: string;
  username: string;
  email: string;
  role: Role;
}
export enum Role {
  Admin = 'Admin',
  User = 'User',
  Guest = 'Guest',
}
