using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimesheetManagement.Domain.Entities;

namespace TimesheetManagement.Infrastructure.Persistence.Configurations;

public class TimesheetEntryConfiguration : IEntityTypeConfiguration<TimesheetEntry>
{
    public void Configure(EntityTypeBuilder<TimesheetEntry> builder)
    {
        builder.HasKey(e => e.Id);

        foreach (var dayProperty in new[] { nameof(TimesheetEntry.MonHours), nameof(TimesheetEntry.TueHours),
                     nameof(TimesheetEntry.WedHours), nameof(TimesheetEntry.ThuHours), nameof(TimesheetEntry.FriHours),
                     nameof(TimesheetEntry.SatHours), nameof(TimesheetEntry.SunHours) })
        {
            builder.Property(dayProperty).HasPrecision(5, 2);
        }

        builder.Property(e => e.Notes).HasMaxLength(1000);
        builder.Ignore(e => e.RowTotal);

        builder.HasOne(e => e.Project)
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ProjectTask)
            .WithMany()
            .HasForeignKey(e => e.ProjectTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.TimesheetWeekId, e.ProjectId, e.ProjectTaskId }).IsUnique();
    }
}
