using TimesheetManagement.Application.Dtos;

namespace TimesheetManagement.Application.Services;

public interface IProjectService
{
    Task<List<ProjectDto>> GetAllAsync(bool includeInactive, CancellationToken ct = default);
    Task<ProjectDto> CreateAsync(Guid createdBy, CreateProjectRequest request, CancellationToken ct = default);
    Task<ProjectDto> UpdateAsync(Guid projectId, UpdateProjectRequest request, CancellationToken ct = default);
    Task<List<ProjectTaskDto>> GetTasksAsync(Guid projectId, bool includeInactive, CancellationToken ct = default);
    Task<ProjectTaskDto> CreateTaskAsync(CreateProjectTaskRequest request, CancellationToken ct = default);
    Task<ProjectTaskDto> UpdateTaskAsync(Guid taskId, UpdateProjectTaskRequest request, CancellationToken ct = default);
}
