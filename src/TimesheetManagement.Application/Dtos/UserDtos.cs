using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Application.Dtos;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    Guid? ManagerId,
    string? ManagerName,
    bool IsActive);

public record UpdateUserRoleRequest(UserRole Role);

public record UpdateUserManagerRequest(Guid? ManagerId);
