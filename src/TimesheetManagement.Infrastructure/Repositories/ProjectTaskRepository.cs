using Microsoft.EntityFrameworkCore;
using TimesheetManagement.Application.Repositories;
using TimesheetManagement.Infrastructure.Persistence;
using ProjectTaskEntity = TimesheetManagement.Domain.Entities.ProjectTask;

namespace TimesheetManagement.Infrastructure.Repositories;

public class ProjectTaskRepository(TimesheetDbContext db) : IProjectTaskRepository
{
    public Task<ProjectTaskEntity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.ProjectTasks.FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<List<ProjectTaskEntity>> GetByProjectIdAsync(Guid projectId, bool includeInactive, CancellationToken ct = default) =>
        db.ProjectTasks.AsNoTracking()
            .Where(t => t.ProjectId == projectId && (includeInactive || t.IsActive))
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

    public Task<List<ProjectTaskEntity>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
        db.ProjectTasks.AsNoTracking().Where(t => ids.Contains(t.Id)).ToListAsync(ct);

    public async Task AddAsync(ProjectTaskEntity task, CancellationToken ct = default) =>
        await db.ProjectTasks.AddAsync(task, ct);
}
