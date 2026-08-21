using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Domain.Entities;

/// <summary>Header row: one per employee per week.</summary>
public class TimesheetWeek
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public DateOnly WeekStartDate { get; set; }
    public DateOnly WeekEndDate { get; set; }

    public TimesheetStatus Status { get; set; } = TimesheetStatus.Draft;

    public DateTime? SubmittedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public User? ApprovedByUser { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionComment { get; set; }

    /// <summary>Denormalized sum of all entries' day hours, recomputed server-side on every save.</summary>
    public decimal TotalHours { get; set; }

    public ICollection<TimesheetEntry> Entries { get; set; } = new List<TimesheetEntry>();
    public ICollection<ApprovalHistory> ApprovalHistories { get; set; } = new List<ApprovalHistory>();
}
