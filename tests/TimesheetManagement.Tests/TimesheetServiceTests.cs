using Microsoft.Extensions.Options;
using TimesheetManagement.Application.Dtos;
using TimesheetManagement.Application.Exceptions;
using TimesheetManagement.Application.Options;
using TimesheetManagement.Application.Services;
using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Domain.Enums;
using TimesheetManagement.Tests.Fakes;
using ProjectTaskEntity = TimesheetManagement.Domain.Entities.ProjectTask;

namespace TimesheetManagement.Tests;

public class TimesheetServiceTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeProjectRepository _projects = new();
    private readonly FakeProjectTaskRepository _tasks = new();
    private readonly FakeTimesheetWeekRepository _weeks = new();
    private readonly FakeApprovalHistoryRepository _history = new();
    private readonly TimesheetService _sut;

    private readonly Guid _managerId = Guid.NewGuid();
    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _otherManagerId = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _taskId = Guid.NewGuid();
    private readonly DateOnly _weekStart;

    public TimesheetServiceTests()
    {
        _sut = new TimesheetService(_weeks, _users, _projects, _tasks, _history, new FakeUnitOfWork(),
            Options.Create(new TimesheetOptions()));

        _weekStart = MostRecentMonday(DateOnly.FromDateTime(DateTime.UtcNow));

        _users.Users.Add(new User { Id = _managerId, FullName = "Mia Manager", Email = "manager@test.local", Role = UserRole.Manager, IsActive = true, ExternalAuthId = "m1" });
        _users.Users.Add(new User { Id = _otherManagerId, FullName = "Other Manager", Email = "other@test.local", Role = UserRole.Manager, IsActive = true, ExternalAuthId = "m2" });
        _users.Users.Add(new User { Id = _employeeId, FullName = "Eli Employee", Email = "employee@test.local", Role = UserRole.Employee, ManagerId = _managerId, IsActive = true, ExternalAuthId = "e1" });

        _projects.Projects.Add(new Project { Id = _projectId, Name = "Project X", Code = "PX-1", IsActive = true, CreatedBy = _managerId, CreatedAt = DateTime.UtcNow });
        _tasks.Tasks.Add(new ProjectTaskEntity { Id = _taskId, ProjectId = _projectId, Name = "Build", Classification = TaskClassification.CapEx, IsBillable = true, IsActive = true, CreatedAt = DateTime.UtcNow });
    }

    private static DateOnly MostRecentMonday(DateOnly from)
    {
        var diff = ((int)from.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return from.AddDays(-diff);
    }

    private SaveTimesheetRequest DraftWithHours(decimal monHours = 8) => new(
        _weekStart,
        [new SaveTimesheetEntryRequest(_projectId, _taskId, monHours, 0, 0, 0, 0, 0, 0, null)]);

    [Fact]
    public async Task SaveDraft_CreatesNewWeek_InDraftStatus()
    {
        var result = await _sut.SaveDraftAsync(_employeeId, DraftWithHours());

        Assert.Equal(TimesheetStatus.Draft, result.Week.Status);
        Assert.Equal(8, result.Week.TotalHours);
        Assert.Single(result.Week.Entries);
    }

    [Fact]
    public async Task Submit_MovesDraftToSubmitted_AndRecordsHistory()
    {
        var saved = await _sut.SaveDraftAsync(_employeeId, DraftWithHours());

        var submitted = await _sut.SubmitAsync(_employeeId, saved.Week.Id);

        Assert.Equal(TimesheetStatus.Submitted, submitted.Status);
        Assert.NotNull(submitted.SubmittedAt);
        Assert.Contains(_history.Histories, h => h.Action == ApprovalAction.Submitted && h.TimesheetWeekId == submitted.Id);
    }

    [Fact]
    public async Task Submit_OnAlreadySubmittedWeek_ThrowsConflict()
    {
        var saved = await _sut.SaveDraftAsync(_employeeId, DraftWithHours());
        await _sut.SubmitAsync(_employeeId, saved.Week.Id);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.SubmitAsync(_employeeId, saved.Week.Id));
    }

    [Fact]
    public async Task SaveDraft_OnSubmittedWeek_ThrowsConflict_LockedForEmployee()
    {
        var saved = await _sut.SaveDraftAsync(_employeeId, DraftWithHours());
        await _sut.SubmitAsync(_employeeId, saved.Week.Id);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.SaveDraftAsync(_employeeId, DraftWithHours(4)));
    }

    [Fact]
    public async Task Approve_ByDirectManager_MovesSubmittedToApproved()
    {
        var saved = await _sut.SaveDraftAsync(_employeeId, DraftWithHours());
        var submitted = await _sut.SubmitAsync(_employeeId, saved.Week.Id);

        var approved = await _sut.ApproveAsync(_managerId, submitted.Id);

        Assert.Equal(TimesheetStatus.Approved, approved.Status);
        Assert.Equal("Mia Manager", approved.ApprovedByName);
        Assert.NotNull(approved.ApprovedAt);
        Assert.Contains(_history.Histories, h => h.Action == ApprovalAction.Approved);
    }

    [Fact]
    public async Task Approve_ByNonManagerOfEmployee_ThrowsForbidden()
    {
        var saved = await _sut.SaveDraftAsync(_employeeId, DraftWithHours());
        var submitted = await _sut.SubmitAsync(_employeeId, saved.Week.Id);

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.ApproveAsync(_otherManagerId, submitted.Id));
    }

    [Fact]
    public async Task Approve_OnDraftWeek_ThrowsConflict()
    {
        var saved = await _sut.SaveDraftAsync(_employeeId, DraftWithHours());

        await Assert.ThrowsAsync<ConflictException>(() => _sut.ApproveAsync(_managerId, saved.Week.Id));
    }

    [Fact]
    public async Task Reject_WithoutComment_ThrowsValidation()
    {
        var saved = await _sut.SaveDraftAsync(_employeeId, DraftWithHours());
        var submitted = await _sut.SubmitAsync(_employeeId, saved.Week.Id);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.RejectAsync(_managerId, submitted.Id, new RejectTimesheetRequest("")));
    }

    [Fact]
    public async Task Reject_WithComment_MovesSubmittedToRejected()
    {
        var saved = await _sut.SaveDraftAsync(_employeeId, DraftWithHours());
        var submitted = await _sut.SubmitAsync(_employeeId, saved.Week.Id);

        var rejected = await _sut.RejectAsync(_managerId, submitted.Id, new RejectTimesheetRequest("Please fix hours."));

        Assert.Equal(TimesheetStatus.Rejected, rejected.Status);
        Assert.Equal("Please fix hours.", rejected.RejectionComment);
        Assert.Contains(_history.Histories, h => h.Action == ApprovalAction.Rejected && h.Comment == "Please fix hours.");
    }

    [Fact]
    public async Task FullCycle_RejectedWeek_CanBeEditedAndResubmitted()
    {
        var saved = await _sut.SaveDraftAsync(_employeeId, DraftWithHours());
        var submitted = await _sut.SubmitAsync(_employeeId, saved.Week.Id);
        await _sut.RejectAsync(_managerId, submitted.Id, new RejectTimesheetRequest("Needs correction."));

        // Employee can still edit while Rejected.
        var edited = await _sut.SaveDraftAsync(_employeeId, DraftWithHours(6));
        Assert.Equal(TimesheetStatus.Rejected, edited.Week.Status);
        Assert.Equal(6, edited.Week.TotalHours);

        var resubmitted = await _sut.SubmitAsync(_employeeId, edited.Week.Id);

        Assert.Equal(TimesheetStatus.Submitted, resubmitted.Status);
        Assert.Null(resubmitted.RejectionComment);
    }

    [Fact]
    public async Task SaveDraft_WithHourAboveWarnThreshold_ReturnsWarningNotError()
    {
        var result = await _sut.SaveDraftAsync(_employeeId, DraftWithHours(12));

        Assert.NotEmpty(result.Warnings);
        Assert.Equal(TimesheetStatus.Draft, result.Week.Status);
    }

    [Fact]
    public async Task SaveDraft_OnWrongWeekStartDay_ThrowsValidation()
    {
        var notMonday = _weekStart.AddDays(1);
        var request = new SaveTimesheetRequest(notMonday, []);

        await Assert.ThrowsAsync<ValidationAppException>(() => _sut.SaveDraftAsync(_employeeId, request));
    }
}
