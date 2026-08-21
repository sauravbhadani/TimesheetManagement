using TimesheetManagement.Application.Dtos;
using TimesheetManagement.Application.Exceptions;
using TimesheetManagement.Application.Repositories;
using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Application.Services;

public class UserService(IUserRepository userRepository, IUnitOfWork unitOfWork) : IUserService
{
    public async Task<List<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetAllAsync(ct);
        var byId = users.ToDictionary(u => u.Id);
        return users.Select(u => Map(u, byId)).ToList();
    }

    public async Task<UserDto> UpdateRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User {userId} was not found.");

        user.Role = request.Role;
        await unitOfWork.SaveChangesAsync(ct);

        var all = await userRepository.GetAllAsync(ct);
        return Map(user, all.ToDictionary(u => u.Id));
    }

    public async Task<UserDto> UpdateManagerAsync(Guid userId, UpdateUserManagerRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User {userId} was not found.");

        if (request.ManagerId == userId)
        {
            throw new ValidationAppException(nameof(request.ManagerId), "A user cannot be their own manager.");
        }

        if (request.ManagerId is Guid managerId)
        {
            var manager = await userRepository.GetByIdAsync(managerId, ct)
                ?? throw new ValidationAppException(nameof(request.ManagerId), "The selected manager does not exist.");

            if (manager.Role != UserRole.Manager && manager.Role != UserRole.Admin)
            {
                throw new ValidationAppException(nameof(request.ManagerId), "The selected user is not a Manager.");
            }
        }

        user.ManagerId = request.ManagerId;
        await unitOfWork.SaveChangesAsync(ct);

        var all = await userRepository.GetAllAsync(ct);
        return Map(user, all.ToDictionary(u => u.Id));
    }

    private static UserDto Map(User user, IReadOnlyDictionary<Guid, User> byId)
    {
        string? managerName = user.ManagerId is Guid mid && byId.TryGetValue(mid, out var manager)
            ? manager.FullName
            : null;

        return new UserDto(user.Id, user.FullName, user.Email, user.Role, user.ManagerId, managerName, user.IsActive);
    }
}
