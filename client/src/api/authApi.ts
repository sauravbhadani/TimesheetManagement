import { apiClient } from "./client";
import type { UserRole } from "./types";

export interface LocalUserOption {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
}

export interface LocalLoginResponse {
  token: string;
  user: LocalUserOption;
}

export interface CurrentUser {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
}

export const authApi = {
  getLocalUsers: () => apiClient.get<LocalUserOption[]>("/api/auth/local-users").then((r) => r.data),
  localLogin: (userId: string) =>
    apiClient.post<LocalLoginResponse>("/api/auth/local-login", { userId }).then((r) => r.data),
  me: () => apiClient.get<CurrentUser>("/api/auth/me").then((r) => r.data),
};
