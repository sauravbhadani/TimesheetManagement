using TimesheetManagement.Domain.Entities;

namespace TimesheetManagement.Application.Repositories;

public interface IProjectTaskRepository
{
    Task<ProjectTask?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ProjectTask>> GetByProjectIdAsync(Guid projectId, bool includeInactive, CancellationToken ct = default);
    Task<List<ProjectTask>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task AddAsync(ProjectTask task, CancellationToken ct = default);
}
