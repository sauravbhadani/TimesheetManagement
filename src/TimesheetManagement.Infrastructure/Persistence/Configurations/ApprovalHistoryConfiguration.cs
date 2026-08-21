using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimesheetManagement.Domain.Entities;

namespace TimesheetManagement.Infrastructure.Persistence.Configurations;

public class ApprovalHistoryConfiguration : IEntityTypeConfiguration<ApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ApprovalHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Action).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(h => h.Comment).HasMaxLength(2000);

        builder.HasOne(h => h.ActionByUser)
            .WithMany()
            .HasForeignKey(h => h.ActionBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
