using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Application.Dtos;

public record TimesheetEntryDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid ProjectTaskId,
    string ProjectTaskName,
    TaskClassification Classification,
    bool IsBillable,
    decimal MonHours,
    decimal TueHours,
    decimal WedHours,
    decimal ThuHours,
    decimal FriHours,
    decimal SatHours,
    decimal SunHours,
    string? Notes,
    decimal RowTotal);

public record TimesheetWeekDto(
    Guid Id,
    Guid UserId,
    string UserFullName,
    DateOnly WeekStartDate,
    DateOnly WeekEndDate,
    TimesheetStatus Status,
    DateTime? SubmittedAt,
    string? ApprovedByName,
    DateTime? ApprovedAt,
    string? RejectionComment,
    decimal TotalHours,
    List<TimesheetEntryDto> Entries);

/// <summary>One grid row from the client: which Project+Task, and hours per day. Upserted by (ProjectId, ProjectTaskId).</summary>
public record SaveTimesheetEntryRequest(
    Guid ProjectId,
    Guid ProjectTaskId,
    decimal MonHours,
    decimal TueHours,
    decimal WedHours,
    decimal ThuHours,
    decimal FriHours,
    decimal SatHours,
    decimal SunHours,
    string? Notes);

public record SaveTimesheetRequest(DateOnly WeekStartDate, List<SaveTimesheetEntryRequest> Entries);

public record SaveTimesheetResult(TimesheetWeekDto Week, List<string> Warnings);

public record RejectTimesheetRequest(string Comment);

public record ApprovalHistoryDto(ApprovalAction Action, string ActionByName, DateTime ActionAt, string? Comment);
