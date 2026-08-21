namespace TimesheetManagement.Domain.Entities;

/// <summary>One row per Project+Task within a week; daily hours are flat columns to keep the grid simple.</summary>
public class TimesheetEntry
{
    public Guid Id { get; set; }

    public Guid TimesheetWeekId { get; set; }
    public TimesheetWeek? TimesheetWeek { get; set; }

    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    public Guid ProjectTaskId { get; set; }
    public ProjectTask? ProjectTask { get; set; }

    public decimal MonHours { get; set; }
    public decimal TueHours { get; set; }
    public decimal WedHours { get; set; }
    public decimal ThuHours { get; set; }
    public decimal FriHours { get; set; }
    public decimal SatHours { get; set; }
    public decimal SunHours { get; set; }

    public string? Notes { get; set; }

    public decimal RowTotal => MonHours + TueHours + WedHours + ThuHours + FriHours + SatHours + SunHours;
}
