using TimesheetManagement.Application.Dtos;

namespace TimesheetManagement.Application.Services;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto> UpdateRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateManagerAsync(Guid userId, UpdateUserManagerRequest request, CancellationToken ct = default);
}
