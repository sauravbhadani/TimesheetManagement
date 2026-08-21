using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetManagement.Application.Dtos;
using TimesheetManagement.Application.Services;

namespace TimesheetManagement.Api.Controllers;

[Route("api/tasks")]
[Authorize(Roles = "Admin")]
public class TasksController(IProjectService projectService) : ApiControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ProjectTaskDto>> Create(CreateProjectTaskRequest request, CancellationToken ct)
    {
        var task = await projectService.CreateTaskAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, task);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectTaskDto>> Update(Guid id, UpdateProjectTaskRequest request, CancellationToken ct) =>
        await projectService.UpdateTaskAsync(id, request, ct);
}
