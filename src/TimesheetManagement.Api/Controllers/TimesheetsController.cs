using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetManagement.Application.Auth;
using TimesheetManagement.Application.Dtos;
using TimesheetManagement.Application.Services;
using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Api.Controllers;

[Route("api/timesheets")]
public class TimesheetsController(ITimesheetService timesheetService, ICurrentUserService currentUser) : ApiControllerBase
{
    [HttpGet("mine")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<ActionResult<TimesheetWeekDto?>> GetMine([FromQuery] DateOnly week, CancellationToken ct) =>
        await timesheetService.GetMineAsync(currentUser.UserId, week, ct);

    [HttpPost]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<ActionResult<SaveTimesheetResult>> SaveDraft(SaveTimesheetRequest request, CancellationToken ct) =>
        await timesheetService.SaveDraftAsync(currentUser.UserId, request, ct);

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<ActionResult<TimesheetWeekDto>> Submit(Guid id, CancellationToken ct) =>
        await timesheetService.SubmitAsync(currentUser.UserId, id, ct);

    [HttpGet("mine/history")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<ActionResult<List<TimesheetWeekDto>>> GetMyHistory(CancellationToken ct) =>
        await timesheetService.GetHistoryAsync(currentUser.UserId, ct);

    [HttpGet("team")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<List<TimesheetWeekDto>>> GetTeam([FromQuery] TimesheetStatus? status, CancellationToken ct) =>
        await timesheetService.GetTeamAsync(currentUser.UserId, status, ct);

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<TimesheetWeekDto>> Approve(Guid id, CancellationToken ct) =>
        await timesheetService.ApproveAsync(currentUser.UserId, id, ct);

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<TimesheetWeekDto>> Reject(Guid id, RejectTimesheetRequest request, CancellationToken ct) =>
        await timesheetService.RejectAsync(currentUser.UserId, id, request, ct);

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<TimesheetWeekDto>>> GetAll([FromQuery] TimesheetStatus? status, CancellationToken ct) =>
        await timesheetService.GetAllAsync(status, ct);

    /// <summary>Single week detail — used by both the Employee grid (own week) and Manager review screen.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Employee,Manager,Admin")]
    public async Task<ActionResult<TimesheetWeekDto>> GetById(Guid id, CancellationToken ct) =>
        await timesheetService.GetDetailAsync(id, currentUser.UserId, currentUser.Role, ct);
}
