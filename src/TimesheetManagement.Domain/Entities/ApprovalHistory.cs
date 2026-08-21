using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Domain.Entities;

public class ApprovalHistory
{
    public Guid Id { get; set; }

    public Guid TimesheetWeekId { get; set; }
    public TimesheetWeek? TimesheetWeek { get; set; }

    public ApprovalAction Action { get; set; }

    public Guid ActionBy { get; set; }
    public User? ActionByUser { get; set; }

    public DateTime ActionAt { get; set; }
    public string? Comment { get; set; }
}
