using TimesheetManagement.Application.Dtos;
using TimesheetManagement.Application.Exceptions;
using TimesheetManagement.Application.Repositories;
using TimesheetManagement.Domain.Entities;

namespace TimesheetManagement.Application.Services;

public class ProjectService(
    IProjectRepository projectRepository,
    IProjectTaskRepository taskRepository,
    IUnitOfWork unitOfWork) : IProjectService
{
    public async Task<List<ProjectDto>> GetAllAsync(bool includeInactive, CancellationToken ct = default)
    {
        var projects = await projectRepository.GetAllAsync(includeInactive, ct);
        return projects.Select(Map).ToList();
    }

    public async Task<ProjectDto> CreateAsync(Guid createdBy, CreateProjectRequest request, CancellationToken ct = default)
    {
        ValidateNameAndCode(request.Name, request.Code);

        if (await projectRepository.CodeExistsAsync(request.Code, null, ct))
        {
            throw new ValidationAppException(nameof(request.Code), $"Project code '{request.Code}' is already in use.");
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Code = request.Code.Trim(),
            Description = request.Description,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        await projectRepository.AddAsync(project, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Map(project);
    }

    public async Task<ProjectDto> UpdateAsync(Guid projectId, UpdateProjectRequest request, CancellationToken ct = default)
    {
        var project = await projectRepository.GetByIdAsync(projectId, ct)
            ?? throw new NotFoundException($"Project {projectId} was not found.");

        ValidateNameAndCode(request.Name, request.Code);

        if (await projectRepository.CodeExistsAsync(request.Code, projectId, ct))
        {
            throw new ValidationAppException(nameof(request.Code), $"Project code '{request.Code}' is already in use.");
        }

        project.Name = request.Name.Trim();
        project.Code = request.Code.Trim();
        project.Description = request.Description;
        project.IsActive = request.IsActive;

        await unitOfWork.SaveChangesAsync(ct);
        return Map(project);
    }

    public async Task<List<ProjectTaskDto>> GetTasksAsync(Guid projectId, bool includeInactive, CancellationToken ct = default)
    {
        var project = await projectRepository.GetByIdAsync(projectId, ct)
            ?? throw new NotFoundException($"Project {projectId} was not found.");

        var tasks = await taskRepository.GetByProjectIdAsync(projectId, includeInactive, ct);
        return tasks.Select(t => Map(t, project.Name)).ToList();
    }

    public async Task<ProjectTaskDto> CreateTaskAsync(CreateProjectTaskRequest request, CancellationToken ct = default)
    {
        var project = await projectRepository.GetByIdAsync(request.ProjectId, ct)
            ?? throw new NotFoundException($"Project {request.ProjectId} was not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationAppException(nameof(request.Name), "Task name is required.");
        }

        var task = new Domain.Entities.ProjectTask
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Name = request.Name.Trim(),
            Description = request.Description,
            Classification = request.Classification,
            IsBillable = request.IsBillable,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await taskRepository.AddAsync(task, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Map(task, project.Name);
    }

    public async Task<ProjectTaskDto> UpdateTaskAsync(Guid taskId, UpdateProjectTaskRequest request, CancellationToken ct = default)
    {
        var task = await taskRepository.GetByIdAsync(taskId, ct)
            ?? throw new NotFoundException($"Task {taskId} was not found.");

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationAppException(nameof(request.Name), "Task name is required.");
        }

        var project = await projectRepository.GetByIdAsync(task.ProjectId, ct)
            ?? throw new NotFoundException($"Project {task.ProjectId} was not found.");

        task.Name = request.Name.Trim();
        task.Description = request.Description;
        task.Classification = request.Classification;
        task.IsBillable = request.IsBillable;
        task.IsActive = request.IsActive;

        await unitOfWork.SaveChangesAsync(ct);
        return Map(task, project.Name);
    }

    private static void ValidateNameAndCode(string name, string code)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(name)) errors[nameof(name)] = ["Project name is required."];
        if (string.IsNullOrWhiteSpace(code)) errors[nameof(code)] = ["Project code is required."];
        if (errors.Count > 0) throw new ValidationAppException(errors);
    }

    private static ProjectDto Map(Project p) => new(p.Id, p.Name, p.Code, p.Description, p.IsActive);

    private static ProjectTaskDto Map(Domain.Entities.ProjectTask t, string projectName) =>
        new(t.Id, t.ProjectId, projectName, t.Name, t.Description, t.Classification, t.IsBillable, t.IsActive);
}
