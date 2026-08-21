using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Domain.Entities;

/// <summary>Named ProjectTask, not Task, to avoid colliding with System.Threading.Tasks.Task.</summary>
public class ProjectTask
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskClassification Classification { get; set; }
    public bool IsBillable { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
