using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    /// <summary>Entra Object Id in EntraId auth mode, or the seeded local user id in Local mode.</summary>
    public string ExternalAuthId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public Guid? ManagerId { get; set; }
    public User? Manager { get; set; }
    public ICollection<User> DirectReports { get; set; } = new List<User>();

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<TimesheetWeek> TimesheetWeeks { get; set; } = new List<TimesheetWeek>();
}
