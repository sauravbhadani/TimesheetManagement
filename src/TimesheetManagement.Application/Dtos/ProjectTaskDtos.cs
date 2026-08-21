using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Application.Dtos;

public record ProjectTaskDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Name,
    string? Description,
    TaskClassification Classification,
    bool IsBillable,
    bool IsActive);

public record CreateProjectTaskRequest(
    Guid ProjectId,
    string Name,
    string? Description,
    TaskClassification Classification,
    bool IsBillable);

public record UpdateProjectTaskRequest(
    string Name,
    string? Description,
    TaskClassification Classification,
    bool IsBillable,
    bool IsActive);
