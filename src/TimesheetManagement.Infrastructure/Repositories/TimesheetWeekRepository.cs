using Microsoft.EntityFrameworkCore;
using TimesheetManagement.Application.Repositories;
using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Domain.Enums;
using TimesheetManagement.Infrastructure.Persistence;

namespace TimesheetManagement.Infrastructure.Repositories;

public class TimesheetWeekRepository(TimesheetDbContext db) : ITimesheetWeekRepository
{
    private IQueryable<TimesheetWeek> WithEntries() =>
        db.TimesheetWeeks
            .Include(w => w.Entries).ThenInclude(e => e.Project)
            .Include(w => w.Entries).ThenInclude(e => e.ProjectTask);

    public Task<TimesheetWeek?> GetByIdWithEntriesAsync(Guid id, CancellationToken ct = default) =>
        WithEntries().FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<TimesheetWeek?> GetByUserAndWeekStartAsync(Guid userId, DateOnly weekStartDate, CancellationToken ct = default) =>
        WithEntries().FirstOrDefaultAsync(w => w.UserId == userId && w.WeekStartDate == weekStartDate, ct);

    public Task<List<TimesheetWeek>> GetHistoryForUserAsync(Guid userId, CancellationToken ct = default) =>
        WithEntries()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.WeekStartDate)
            .ToListAsync(ct);

    public Task<List<TimesheetWeek>> GetForUsersAsync(IEnumerable<Guid> userIds, TimesheetStatus? status, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        var query = WithEntries().Where(w => ids.Contains(w.UserId));
        if (status is TimesheetStatus s) query = query.Where(w => w.Status == s);
        return query.OrderByDescending(w => w.WeekStartDate).ToListAsync(ct);
    }

    public Task<List<TimesheetWeek>> GetAllAsync(TimesheetStatus? status, CancellationToken ct = default)
    {
        var query = WithEntries().AsQueryable();
        if (status is TimesheetStatus s) query = query.Where(w => w.Status == s);
        return query.OrderByDescending(w => w.WeekStartDate).ToListAsync(ct);
    }

    public async Task AddAsync(TimesheetWeek week, CancellationToken ct = default) =>
        await db.TimesheetWeeks.AddAsync(week, ct);
}
