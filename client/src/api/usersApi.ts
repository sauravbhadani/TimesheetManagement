import { apiClient } from "./client";
import type { UserDto, UserRole } from "./types";

export const usersApi = {
  getAll: () => apiClient.get<UserDto[]>("/api/users").then((r) => r.data),
  updateRole: (id: string, role: UserRole) =>
    apiClient.put<UserDto>(`/api/users/${id}/role`, { role }).then((r) => r.data),
  updateManager: (id: string, managerId: string | null) =>
    apiClient.put<UserDto>(`/api/users/${id}/manager`, { managerId }).then((r) => r.data),
};
