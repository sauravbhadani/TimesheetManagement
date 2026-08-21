using Microsoft.EntityFrameworkCore;
using TimesheetManagement.Domain.Entities;

namespace TimesheetManagement.Infrastructure.Persistence;

public class TimesheetDbContext(DbContextOptions<TimesheetDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
    public DbSet<TimesheetWeek> TimesheetWeeks => Set<TimesheetWeek>();
    public DbSet<TimesheetEntry> TimesheetEntries => Set<TimesheetEntry>();
    public DbSet<ApprovalHistory> ApprovalHistories => Set<ApprovalHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TimesheetDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
