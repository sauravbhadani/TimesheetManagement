using TimesheetManagement.Domain.Entities;

namespace TimesheetManagement.Application.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Project?> GetByIdWithTasksAsync(Guid id, CancellationToken ct = default);
    Task<List<Project>> GetAllAsync(bool includeInactive, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
}
