using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimesheetManagement.Domain.Entities;

namespace TimesheetManagement.Infrastructure.Persistence.Configurations;

public class TimesheetWeekConfiguration : IEntityTypeConfiguration<TimesheetWeek>
{
    public void Configure(EntityTypeBuilder<TimesheetWeek> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(w => w.RejectionComment).HasMaxLength(2000);
        builder.Property(w => w.TotalHours).HasPrecision(7, 2);

        builder.HasIndex(w => new { w.UserId, w.WeekStartDate }).IsUnique();

        builder.HasOne(w => w.User)
            .WithMany(u => u.TimesheetWeeks)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.ApprovedByUser)
            .WithMany()
            .HasForeignKey(w => w.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.Entries)
            .WithOne(e => e.TimesheetWeek)
            .HasForeignKey(e => e.TimesheetWeekId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.ApprovalHistories)
            .WithOne(h => h.TimesheetWeek)
            .HasForeignKey(h => h.TimesheetWeekId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
