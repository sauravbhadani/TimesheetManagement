using Microsoft.Extensions.Options;
using TimesheetManagement.Application.Dtos;
using TimesheetManagement.Application.Exceptions;
using TimesheetManagement.Application.Options;
using TimesheetManagement.Application.Repositories;
using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Application.Services;

public class TimesheetService(
    ITimesheetWeekRepository weekRepository,
    IUserRepository userRepository,
    IProjectRepository projectRepository,
    IProjectTaskRepository taskRepository,
    IApprovalHistoryRepository historyRepository,
    IUnitOfWork unitOfWork,
    IOptions<TimesheetOptions> timesheetOptions) : ITimesheetService
{
    private static readonly DayOfWeek[] WeekOrder =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    private TimesheetOptions Options => timesheetOptions.Value;

    public async Task<TimesheetWeekDto?> GetMineAsync(Guid userId, DateOnly weekStartDate, CancellationToken ct = default)
    {
        ValidateWeekStart(weekStartDate);
        var week = await weekRepository.GetByUserAndWeekStartAsync(userId, weekStartDate, ct);
        return week is null ? null : await MapAsync(week, ct);
    }

    public async Task<SaveTimesheetResult> SaveDraftAsync(Guid userId, SaveTimesheetRequest request, CancellationToken ct = default)
    {
        ValidateWeekStart(request.WeekStartDate);

        var user = await userRepository.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException($"User {userId} was not found.");

        var week = await weekRepository.GetByUserAndWeekStartAsync(userId, request.WeekStartDate, ct);

        if (week is not null && week.Status is not (TimesheetStatus.Draft or TimesheetStatus.Rejected))
        {
            throw new ConflictException($"Cannot edit a timesheet in {week.Status} status.");
        }

        if (week is null)
        {
            week = new TimesheetWeek
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WeekStartDate = request.WeekStartDate,
                WeekEndDate = request.WeekStartDate.AddDays(6),
                Status = TimesheetStatus.Draft
            };
            await weekRepository.AddAsync(week, ct);
        }

        var warnings = new List<string>();
        await ApplyEntriesAsync(week, request.Entries, warnings, ct);

        week.TotalHours = week.Entries.Sum(e => e.RowTotal);

        await unitOfWork.SaveChangesAsync(ct);

        var dto = await MapAsync(week, ct);
        return new SaveTimesheetResult(dto, warnings);
    }

    public async Task<TimesheetWeekDto> SubmitAsync(Guid userId, Guid weekId, CancellationToken ct = default)
    {
        var week = await weekRepository.GetByIdWithEntriesAsync(weekId, ct)
            ?? throw new NotFoundException($"Timesheet {weekId} was not found.");

        if (week.UserId != userId)
        {
            throw new ForbiddenException("You can only submit your own timesheet.");
        }

        if (week.Status is not (TimesheetStatus.Draft or TimesheetStatus.Rejected))
        {
            throw new ConflictException($"Cannot submit a timesheet in {week.Status} status.");
        }

        week.Status = TimesheetStatus.Submitted;
        week.SubmittedAt = DateTime.UtcNow;
        week.RejectionComment = null;

        await historyRepository.AddAsync(new ApprovalHistory
        {
            Id = Guid.NewGuid(),
            TimesheetWeekId = week.Id,
            Action = ApprovalAction.Submitted,
            ActionBy = userId,
            ActionAt = DateTime.UtcNow
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);
        return await MapAsync(week, ct);
    }

    public async Task<List<TimesheetWeekDto>> GetHistoryAsync(Guid userId, CancellationToken ct = default)
    {
        var weeks = await weekRepository.GetHistoryForUserAsync(userId, ct);
        return await MapAllAsync(weeks, ct);
    }

    public async Task<List<TimesheetWeekDto>> GetTeamAsync(Guid managerId, TimesheetStatus? status, CancellationToken ct = default)
    {
        var reports = await userRepository.GetDirectReportsAsync(managerId, ct);
        var weeks = await weekRepository.GetForUsersAsync(reports.Select(r => r.Id), status, ct);
        return await MapAllAsync(weeks, ct);
    }

    public async Task<TimesheetWeekDto> ApproveAsync(Guid managerId, Guid weekId, CancellationToken ct = default)
    {
        var week = await GetSubmittedWeekOwnedByManagerAsync(managerId, weekId, ct);

        week.Status = TimesheetStatus.Approved;
        week.ApprovedBy = managerId;
        week.ApprovedAt = DateTime.UtcNow;

        await historyRepository.AddAsync(new ApprovalHistory
        {
            Id = Guid.NewGuid(),
            TimesheetWeekId = week.Id,
            Action = ApprovalAction.Approved,
            ActionBy = managerId,
            ActionAt = DateTime.UtcNow
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);
        return await MapAsync(week, ct);
    }

    public async Task<TimesheetWeekDto> RejectAsync(Guid managerId, Guid weekId, RejectTimesheetRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            throw new ValidationAppException(nameof(request.Comment), "A comment is required to reject a timesheet.");
        }

        var week = await GetSubmittedWeekOwnedByManagerAsync(managerId, weekId, ct);

        week.Status = TimesheetStatus.Rejected;
        week.RejectionComment = request.Comment.Trim();

        await historyRepository.AddAsync(new ApprovalHistory
        {
            Id = Guid.NewGuid(),
            TimesheetWeekId = week.Id,
            Action = ApprovalAction.Rejected,
            ActionBy = managerId,
            ActionAt = DateTime.UtcNow,
            Comment = week.RejectionComment
        }, ct);

        await unitOfWork.SaveChangesAsync(ct);
        return await MapAsync(week, ct);
    }

    public async Task<List<TimesheetWeekDto>> GetAllAsync(TimesheetStatus? status, CancellationToken ct = default)
    {
        var weeks = await weekRepository.GetAllAsync(status, ct);
        return await MapAllAsync(weeks, ct);
    }

    public async Task<TimesheetWeekDto> GetDetailAsync(Guid weekId, Guid requestingUserId, UserRole requestingRole, CancellationToken ct = default)
    {
        var week = await weekRepository.GetByIdWithEntriesAsync(weekId, ct)
            ?? throw new NotFoundException($"Timesheet {weekId} was not found.");

        var allowed = requestingRole switch
        {
            UserRole.Admin => true,
            UserRole.Employee => week.UserId == requestingUserId,
            UserRole.Manager => week.UserId == requestingUserId || await IsDirectReportAsync(requestingUserId, week.UserId, ct),
            _ => false
        };

        if (!allowed)
        {
            throw new ForbiddenException("You do not have access to this timesheet.");
        }

        return await MapAsync(week, ct);
    }

    private async Task<bool> IsDirectReportAsync(Guid managerId, Guid userId, CancellationToken ct)
    {
        var reports = await userRepository.GetDirectReportsAsync(managerId, ct);
        return reports.Any(r => r.Id == userId);
    }

    private async Task<TimesheetWeek> GetSubmittedWeekOwnedByManagerAsync(Guid managerId, Guid weekId, CancellationToken ct)
    {
        var week = await weekRepository.GetByIdWithEntriesAsync(weekId, ct)
            ?? throw new NotFoundException($"Timesheet {weekId} was not found.");

        if (!await IsDirectReportAsync(managerId, week.UserId, ct))
        {
            throw new ForbiddenException("You can only act on timesheets submitted by your direct reports.");
        }

        if (week.Status != TimesheetStatus.Submitted)
        {
            throw new ConflictException($"Cannot act on a timesheet in {week.Status} status; it must be Submitted.");
        }

        return week;
    }

    private async Task ApplyEntriesAsync(
        TimesheetWeek week,
        List<SaveTimesheetEntryRequest> requestedEntries,
        List<string> warnings,
        CancellationToken ct)
    {
        // Reject duplicate Project+Task rows in the same submission up front.
        var duplicateKey = requestedEntries
            .GroupBy(e => (e.ProjectId, e.ProjectTaskId))
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateKey is not null)
        {
            throw new ValidationAppException("Entries", "Each Project + Task combination can only appear once per week.");
        }

        foreach (var entry in requestedEntries)
        {
            ValidateNonNegativeHours(entry);
        }

        // Remove rows the client no longer has (grid is the full source of truth for the week).
        var incomingKeys = requestedEntries.Select(e => (e.ProjectId, e.ProjectTaskId)).ToHashSet();
        var toRemove = week.Entries.Where(e => !incomingKeys.Contains((e.ProjectId, e.ProjectTaskId))).ToList();
        foreach (var stale in toRemove)
        {
            week.Entries.Remove(stale);
        }

        foreach (var requested in requestedEntries)
        {
            var existing = week.Entries.FirstOrDefault(e => e.ProjectId == requested.ProjectId && e.ProjectTaskId == requested.ProjectTaskId);

            if (existing is null)
            {
                var project = await projectRepository.GetByIdAsync(requested.ProjectId, ct)
                    ?? throw new ValidationAppException(nameof(requested.ProjectId), "Selected project does not exist.");
                var task = await taskRepository.GetByIdAsync(requested.ProjectTaskId, ct)
                    ?? throw new ValidationAppException(nameof(requested.ProjectTaskId), "Selected task does not exist.");

                if (task.ProjectId != project.Id)
                {
                    throw new ValidationAppException(nameof(requested.ProjectTaskId), "Task does not belong to the selected project.");
                }
                if (!project.IsActive || !task.IsActive)
                {
                    throw new ValidationAppException(nameof(requested.ProjectTaskId), "Cannot log new hours against an inactive project or task.");
                }

                existing = new TimesheetEntry
                {
                    Id = Guid.NewGuid(),
                    TimesheetWeekId = week.Id,
                    ProjectId = requested.ProjectId,
                    ProjectTaskId = requested.ProjectTaskId,
                    Project = project,
                    ProjectTask = task
                };
                week.Entries.Add(existing);
            }

            existing.MonHours = requested.MonHours;
            existing.TueHours = requested.TueHours;
            existing.WedHours = requested.WedHours;
            existing.ThuHours = requested.ThuHours;
            existing.FriHours = requested.FriHours;
            existing.SatHours = requested.SatHours;
            existing.SunHours = requested.SunHours;
            existing.Notes = requested.Notes;
        }

        ValidateDailyTotals(week.Entries, warnings);
    }

    private static void ValidateNonNegativeHours(SaveTimesheetEntryRequest entry)
    {
        decimal[] hours = [entry.MonHours, entry.TueHours, entry.WedHours, entry.ThuHours, entry.FriHours, entry.SatHours, entry.SunHours];
        if (hours.Any(h => h < 0))
        {
            throw new ValidationAppException("Hours", "Hours cannot be negative.");
        }
    }

    private void ValidateDailyTotals(IEnumerable<TimesheetEntry> entries, List<string> warnings)
    {
        var entryList = entries.ToList();
        var dailyTotals = new Dictionary<DayOfWeek, decimal>
        {
            [DayOfWeek.Monday] = entryList.Sum(e => e.MonHours),
            [DayOfWeek.Tuesday] = entryList.Sum(e => e.TueHours),
            [DayOfWeek.Wednesday] = entryList.Sum(e => e.WedHours),
            [DayOfWeek.Thursday] = entryList.Sum(e => e.ThuHours),
            [DayOfWeek.Friday] = entryList.Sum(e => e.FriHours),
            [DayOfWeek.Saturday] = entryList.Sum(e => e.SatHours),
            [DayOfWeek.Sunday] = entryList.Sum(e => e.SunHours)
        };

        foreach (var day in WeekOrder)
        {
            var total = dailyTotals[day];

            if (total > Options.MaxHoursPerDay)
            {
                if (Options.EnforceHardCap)
                {
                    throw new ValidationAppException(day.ToString(), $"{day} total of {total}h exceeds the {Options.MaxHoursPerDay}h/day limit.");
                }
                warnings.Add($"{day} total is {total}h, above the {Options.MaxHoursPerDay}h/day limit.");
            }
            else if (total > Options.WarnHoursPerDayThreshold)
            {
                warnings.Add($"{day} total is {total}h, above the usual {Options.WarnHoursPerDayThreshold}h/day.");
            }
        }
    }

    private void ValidateWeekStart(DateOnly weekStartDate)
    {
        if (weekStartDate.DayOfWeek != Options.WeekStartDayOfWeek)
        {
            throw new ValidationAppException(nameof(weekStartDate), $"Week must start on a {Options.WeekStartDayOfWeek}.");
        }
    }

    private async Task<TimesheetWeekDto> MapAsync(TimesheetWeek week, CancellationToken ct)
    {
        var userIds = new HashSet<Guid> { week.UserId };
        if (week.ApprovedBy is Guid approvedBy) userIds.Add(approvedBy);

        var names = await ResolveNamesAsync(userIds, ct);
        return MapWithNames(week, names);
    }

    private static TimesheetWeekDto MapWithNames(TimesheetWeek week, IReadOnlyDictionary<Guid, string> names)
    {
        var entries = week.Entries
            .OrderBy(e => e.Project!.Name)
            .ThenBy(e => e.ProjectTask!.Name)
            .Select(e => new TimesheetEntryDto(
                e.Id,
                e.ProjectId,
                e.Project?.Name ?? string.Empty,
                e.ProjectTaskId,
                e.ProjectTask?.Name ?? string.Empty,
                e.ProjectTask?.Classification ?? TaskClassification.OpEx,
                e.ProjectTask?.IsBillable ?? false,
                e.MonHours, e.TueHours, e.WedHours, e.ThuHours, e.FriHours, e.SatHours, e.SunHours,
                e.Notes,
                e.RowTotal))
            .ToList();

        return new TimesheetWeekDto(
            week.Id,
            week.UserId,
            names.GetValueOrDefault(week.UserId, string.Empty),
            week.WeekStartDate,
            week.WeekEndDate,
            week.Status,
            week.SubmittedAt,
            week.ApprovedBy is Guid ab ? names.GetValueOrDefault(ab) : null,
            week.ApprovedAt,
            week.RejectionComment,
            week.TotalHours,
            entries);
    }

    private async Task<List<TimesheetWeekDto>> MapAllAsync(List<TimesheetWeek> weeks, CancellationToken ct)
    {
        var userIds = new HashSet<Guid>();
        foreach (var week in weeks)
        {
            userIds.Add(week.UserId);
            if (week.ApprovedBy is Guid approvedBy) userIds.Add(approvedBy);
        }

        var names = await ResolveNamesAsync(userIds, ct);
        return weeks.Select(w => MapWithNames(w, names)).ToList();
    }

    private async Task<Dictionary<Guid, string>> ResolveNamesAsync(IEnumerable<Guid> userIds, CancellationToken ct)
    {
        var allUsers = await userRepository.GetAllAsync(ct);
        var byId = allUsers.ToDictionary(u => u.Id, u => u.FullName);
        return userIds.Where(byId.ContainsKey).ToDictionary(id => id, id => byId[id]);
    }
}
