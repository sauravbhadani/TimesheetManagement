export type UserRole = "Admin" | "Manager" | "Employee";
export type TaskClassification = "CapEx" | "OpEx";
export type TimesheetStatus = "Draft" | "Submitted" | "Approved" | "Rejected";
export type ApprovalAction = "Submitted" | "Approved" | "Rejected";

export interface UserDto {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  managerId: string | null;
  managerName: string | null;
  isActive: boolean;
}

export interface ProjectDto {
  id: string;
  name: string;
  code: string;
  description: string | null;
  isActive: boolean;
}

export interface ProjectTaskDto {
  id: string;
  projectId: string;
  projectName: string;
  name: string;
  description: string | null;
  classification: TaskClassification;
  isBillable: boolean;
  isActive: boolean;
}

export interface TimesheetEntryDto {
  id: string;
  projectId: string;
  projectName: string;
  projectTaskId: string;
  projectTaskName: string;
  classification: TaskClassification;
  isBillable: boolean;
  monHours: number;
  tueHours: number;
  wedHours: number;
  thuHours: number;
  friHours: number;
  satHours: number;
  sunHours: number;
  notes: string | null;
  rowTotal: number;
}

export interface TimesheetWeekDto {
  id: string;
  userId: string;
  userFullName: string;
  weekStartDate: string; // yyyy-MM-dd
  weekEndDate: string;
  status: TimesheetStatus;
  submittedAt: string | null;
  approvedByName: string | null;
  approvedAt: string | null;
  rejectionComment: string | null;
  totalHours: number;
  entries: TimesheetEntryDto[];
}

export interface SaveTimesheetEntryRequest {
  projectId: string;
  projectTaskId: string;
  monHours: number;
  tueHours: number;
  wedHours: number;
  thuHours: number;
  friHours: number;
  satHours: number;
  sunHours: number;
  notes: string | null;
}

export interface SaveTimesheetRequest {
  weekStartDate: string;
  entries: SaveTimesheetEntryRequest[];
}

export interface SaveTimesheetResult {
  week: TimesheetWeekDto;
  warnings: string[];
}

export interface RejectTimesheetRequest {
  comment: string;
}

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}
