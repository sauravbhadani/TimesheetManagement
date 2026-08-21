namespace TimesheetManagement.Application.Dtos;

public record ProjectDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive);

public record CreateProjectRequest(string Name, string Code, string? Description);

public record UpdateProjectRequest(string Name, string Code, string? Description, bool IsActive);
