using TimesheetManagement.Application.Dtos;
using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Application.Services;

public interface ITimesheetService
{
    /// <summary>Null when the employee has not started this week yet — the client renders a blank grid.</summary>
    Task<TimesheetWeekDto?> GetMineAsync(Guid userId, DateOnly weekStartDate, CancellationToken ct = default);

    /// <summary>Creates or updates the Draft/Rejected week for (userId, request.WeekStartDate). Never changes Status.</summary>
    Task<SaveTimesheetResult> SaveDraftAsync(Guid userId, SaveTimesheetRequest request, CancellationToken ct = default);

    Task<TimesheetWeekDto> SubmitAsync(Guid userId, Guid weekId, CancellationToken ct = default);

    Task<List<TimesheetWeekDto>> GetHistoryAsync(Guid userId, CancellationToken ct = default);

    Task<List<TimesheetWeekDto>> GetTeamAsync(Guid managerId, TimesheetStatus? status, CancellationToken ct = default);

    Task<TimesheetWeekDto> ApproveAsync(Guid managerId, Guid weekId, CancellationToken ct = default);

    Task<TimesheetWeekDto> RejectAsync(Guid managerId, Guid weekId, RejectTimesheetRequest request, CancellationToken ct = default);

    Task<List<TimesheetWeekDto>> GetAllAsync(TimesheetStatus? status, CancellationToken ct = default);

    /// <summary>Single week, with resource-level authorization: owner, their manager, or Admin.</summary>
    Task<TimesheetWeekDto> GetDetailAsync(Guid weekId, Guid requestingUserId, UserRole requestingRole, CancellationToken ct = default);
}
