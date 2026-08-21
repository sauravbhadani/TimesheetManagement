using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetManagement.Application.Auth;
using TimesheetManagement.Application.Dtos;
using TimesheetManagement.Application.Services;

namespace TimesheetManagement.Api.Controllers;

[Route("api/projects")]
public class ProjectsController(IProjectService projectService, ICurrentUserService currentUser) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProjectDto>>> GetAll([FromQuery] bool includeInactive, CancellationToken ct) =>
        await projectService.GetAllAsync(includeInactive, ct);

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProjectDto>> Create(CreateProjectRequest request, CancellationToken ct)
    {
        var project = await projectService.CreateAsync(currentUser.UserId, request, ct);
        return StatusCode(StatusCodes.Status201Created, project);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, UpdateProjectRequest request, CancellationToken ct) =>
        await projectService.UpdateAsync(id, request, ct);

    [HttpGet("{id:guid}/tasks")]
    public async Task<ActionResult<List<ProjectTaskDto>>> GetTasks(Guid id, [FromQuery] bool includeInactive, CancellationToken ct) =>
        await projectService.GetTasksAsync(id, includeInactive, ct);
}
