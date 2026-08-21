using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Application.Repositories;

public interface ITimesheetWeekRepository
{
    /// <summary>Loads a week with its Entries (and each entry's Project/ProjectTask) for editing or review.</summary>
    Task<TimesheetWeek?> GetByIdWithEntriesAsync(Guid id, CancellationToken ct = default);

    Task<TimesheetWeek?> GetByUserAndWeekStartAsync(Guid userId, DateOnly weekStartDate, CancellationToken ct = default);

    Task<List<TimesheetWeek>> GetHistoryForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Weeks belonging to any of the given userIds (a manager's direct reports), optionally filtered by status.</summary>
    Task<List<TimesheetWeek>> GetForUsersAsync(IEnumerable<Guid> userIds, TimesheetStatus? status, CancellationToken ct = default);

    Task<List<TimesheetWeek>> GetAllAsync(TimesheetStatus? status, CancellationToken ct = default);

    Task AddAsync(TimesheetWeek week, CancellationToken ct = default);
}
