import { apiClient } from "./client";
import type { ProjectDto, ProjectTaskDto, TaskClassification } from "./types";

export interface CreateProjectRequest {
  name: string;
  code: string;
  description: string | null;
}

export interface UpdateProjectRequest extends CreateProjectRequest {
  isActive: boolean;
}

export interface CreateProjectTaskRequest {
  projectId: string;
  name: string;
  description: string | null;
  classification: TaskClassification;
  isBillable: boolean;
}

export interface UpdateProjectTaskRequest {
  name: string;
  description: string | null;
  classification: TaskClassification;
  isBillable: boolean;
  isActive: boolean;
}

export const projectsApi = {
  getAll: (includeInactive = false) =>
    apiClient.get<ProjectDto[]>("/api/projects", { params: { includeInactive } }).then((r) => r.data),
  create: (request: CreateProjectRequest) =>
    apiClient.post<ProjectDto>("/api/projects", request).then((r) => r.data),
  update: (id: string, request: UpdateProjectRequest) =>
    apiClient.put<ProjectDto>(`/api/projects/${id}`, request).then((r) => r.data),
  getTasks: (projectId: string, includeInactive = false) =>
    apiClient
      .get<ProjectTaskDto[]>(`/api/projects/${projectId}/tasks`, { params: { includeInactive } })
      .then((r) => r.data),
  createTask: (request: CreateProjectTaskRequest) =>
    apiClient.post<ProjectTaskDto>("/api/tasks", request).then((r) => r.data),
  updateTask: (id: string, request: UpdateProjectTaskRequest) =>
    apiClient.put<ProjectTaskDto>(`/api/tasks/${id}`, request).then((r) => r.data),
};
