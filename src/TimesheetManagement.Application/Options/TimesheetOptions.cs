namespace TimesheetManagement.Application.Options;

/// <summary>
/// Business rules that vary by customer without needing a code change.
/// Bound from the "Timesheet" section of appsettings.json.
/// </summary>
public class TimesheetOptions
{
    public const string SectionName = "Timesheet";

    /// <summary>First day of the configured work week. Defaults to Monday per Section 4 of the brief.</summary>
    public DayOfWeek WeekStartDayOfWeek { get; set; } = DayOfWeek.Monday;

    /// <summary>Absolute ceiling for hours logged against a single day.</summary>
    public decimal MaxHoursPerDay { get; set; } = 24;

    /// <summary>Hours per day above which a soft warning is surfaced to the caller.</summary>
    public decimal WarnHoursPerDayThreshold { get; set; } = 10;

    /// <summary>When true, exceeding MaxHoursPerDay blocks the save instead of just warning.</summary>
    public bool EnforceHardCap { get; set; } = false;
}
