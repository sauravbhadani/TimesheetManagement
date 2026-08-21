using TimesheetManagement.Application.Repositories;
using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Tests.Fakes;

/// <summary>
/// Minimal in-memory stand-ins for the repository interfaces, used to exercise TimesheetService's
/// state-machine logic without a real database. No mocking framework needed for behavior this simple.
/// </summary>
public class FakeUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
}

public class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByExternalAuthIdAsync(string externalAuthId, CancellationToken ct = default) =>
        Task.FromResult(Users.FirstOrDefault(u => u.ExternalAuthId == externalAuthId));

    public Task<List<User>> GetAllAsync(CancellationToken ct = default) => Task.FromResult(Users.ToList());

    public Task<List<User>> GetDirectReportsAsync(Guid managerId, CancellationToken ct = default) =>
        Task.FromResult(Users.Where(u => u.ManagerId == managerId).ToList());

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }
}

public class FakeProjectRepository : IProjectRepository
{
    public List<Project> Projects { get; } = [];

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Projects.FirstOrDefault(p => p.Id == id));

    public Task<Project?> GetByIdWithTasksAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Projects.FirstOrDefault(p => p.Id == id));

    public Task<List<Project>> GetAllAsync(bool includeInactive, CancellationToken ct = default) =>
        Task.FromResult(Projects.Where(p => includeInactive || p.IsActive).ToList());

    public Task<bool> CodeExistsAsync(string code, Guid? excludeId, CancellationToken ct = default) =>
        Task.FromResult(Projects.Any(p => p.Code == code && (excludeId == null || p.Id != excludeId)));

    public Task AddAsync(Project project, CancellationToken ct = default)
    {
        Projects.Add(project);
        return Task.CompletedTask;
    }
}

public class FakeProjectTaskRepository : IProjectTaskRepository
{
    public List<ProjectTask> Tasks { get; } = [];

    public Task<ProjectTask?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Tasks.FirstOrDefault(t => t.Id == id));

    public Task<List<ProjectTask>> GetByProjectIdAsync(Guid projectId, bool includeInactive, CancellationToken ct = default) =>
        Task.FromResult(Tasks.Where(t => t.ProjectId == projectId && (includeInactive || t.IsActive)).ToList());

    public Task<List<ProjectTask>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
        Task.FromResult(Tasks.Where(t => ids.Contains(t.Id)).ToList());

    public Task AddAsync(ProjectTask task, CancellationToken ct = default)
    {
        Tasks.Add(task);
        return Task.CompletedTask;
    }
}

public class FakeTimesheetWeekRepository : ITimesheetWeekRepository
{
    public List<TimesheetWeek> Weeks { get; } = [];

    public Task<TimesheetWeek?> GetByIdWithEntriesAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Weeks.FirstOrDefault(w => w.Id == id));

    public Task<TimesheetWeek?> GetByUserAndWeekStartAsync(Guid userId, DateOnly weekStartDate, CancellationToken ct = default) =>
        Task.FromResult(Weeks.FirstOrDefault(w => w.UserId == userId && w.WeekStartDate == weekStartDate));

    public Task<List<TimesheetWeek>> GetHistoryForUserAsync(Guid userId, CancellationToken ct = default) =>
        Task.FromResult(Weeks.Where(w => w.UserId == userId).OrderByDescending(w => w.WeekStartDate).ToList());

    public Task<List<TimesheetWeek>> GetForUsersAsync(IEnumerable<Guid> userIds, TimesheetStatus? status, CancellationToken ct = default)
    {
        var ids = userIds.ToList();
        var query = Weeks.Where(w => ids.Contains(w.UserId));
        if (status is TimesheetStatus s) query = query.Where(w => w.Status == s);
        return Task.FromResult(query.OrderByDescending(w => w.WeekStartDate).ToList());
    }

    public Task<List<TimesheetWeek>> GetAllAsync(TimesheetStatus? status, CancellationToken ct = default)
    {
        var query = Weeks.AsEnumerable();
        if (status is TimesheetStatus s) query = query.Where(w => w.Status == s);
        return Task.FromResult(query.OrderByDescending(w => w.WeekStartDate).ToList());
    }

    public Task AddAsync(TimesheetWeek week, CancellationToken ct = default)
    {
        Weeks.Add(week);
        return Task.CompletedTask;
    }
}

public class FakeApprovalHistoryRepository : IApprovalHistoryRepository
{
    public List<ApprovalHistory> Histories { get; } = [];

    public Task AddAsync(ApprovalHistory history, CancellationToken ct = default)
    {
        Histories.Add(history);
        return Task.CompletedTask;
    }
}
