using Microsoft.EntityFrameworkCore;
using TimesheetManagement.Application.Repositories;
using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Infrastructure.Persistence;

namespace TimesheetManagement.Infrastructure.Repositories;

public class ProjectRepository(TimesheetDbContext db) : IProjectRepository
{
    public Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Project?> GetByIdWithTasksAsync(Guid id, CancellationToken ct = default) =>
        db.Projects.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Project>> GetAllAsync(bool includeInactive, CancellationToken ct = default) =>
        db.Projects.AsNoTracking()
            .Where(p => includeInactive || p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct = default) =>
        db.Projects.AnyAsync(p => p.Code == code && (excludeId == null || p.Id != excludeId), ct);

    public async Task AddAsync(Project project, CancellationToken ct = default) =>
        await db.Projects.AddAsync(project, ct);
}
