import { apiClient } from "./client";
import type {
  RejectTimesheetRequest,
  SaveTimesheetRequest,
  SaveTimesheetResult,
  TimesheetStatus,
  TimesheetWeekDto,
} from "./types";

export const timesheetsApi = {
  getMine: (weekStartDate: string) =>
    apiClient
      .get<TimesheetWeekDto | null>("/api/timesheets/mine", { params: { week: weekStartDate } })
      .then((r) => r.data),
  saveDraft: (request: SaveTimesheetRequest) =>
    apiClient.post<SaveTimesheetResult>("/api/timesheets", request).then((r) => r.data),
  submit: (id: string) => apiClient.post<TimesheetWeekDto>(`/api/timesheets/${id}/submit`).then((r) => r.data),
  getMyHistory: () => apiClient.get<TimesheetWeekDto[]>("/api/timesheets/mine/history").then((r) => r.data),
  getById: (id: string) => apiClient.get<TimesheetWeekDto>(`/api/timesheets/${id}`).then((r) => r.data),
  getTeam: (status?: TimesheetStatus) =>
    apiClient.get<TimesheetWeekDto[]>("/api/timesheets/team", { params: { status } }).then((r) => r.data),
  approve: (id: string) => apiClient.post<TimesheetWeekDto>(`/api/timesheets/${id}/approve`).then((r) => r.data),
  reject: (id: string, request: RejectTimesheetRequest) =>
    apiClient.post<TimesheetWeekDto>(`/api/timesheets/${id}/reject`, request).then((r) => r.data),
  getAll: (status?: TimesheetStatus) =>
    apiClient.get<TimesheetWeekDto[]>("/api/timesheets/all", { params: { status } }).then((r) => r.data),
};
