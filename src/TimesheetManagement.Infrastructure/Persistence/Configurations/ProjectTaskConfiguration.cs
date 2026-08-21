using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimesheetManagement.Domain.Enums;
using TimesheetManagement.Infrastructure.Persistence.Seed;
using ProjectTaskEntity = TimesheetManagement.Domain.Entities.ProjectTask;

namespace TimesheetManagement.Infrastructure.Persistence.Configurations;

public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTaskEntity>
{
    public void Configure(EntityTypeBuilder<ProjectTaskEntity> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.Property(t => t.Classification).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new
            {
                Id = SeedIds.PlatformMaintenanceTaskId,
                ProjectId = SeedIds.InternalToolsProjectId,
                Name = "Platform Maintenance",
                Description = "Keep-the-lights-on maintenance work.",
                Classification = TaskClassification.OpEx,
                IsBillable = false,
                IsActive = true,
                CreatedAt = SeedIds.SeedCreatedAt
            },
            new
            {
                Id = SeedIds.NewFeatureDevTaskId,
                ProjectId = SeedIds.InternalToolsProjectId,
                Name = "New Feature Development",
                Description = "New capability build-out.",
                Classification = TaskClassification.CapEx,
                IsBillable = false,
                IsActive = true,
                CreatedAt = SeedIds.SeedCreatedAt
            },
            new
            {
                Id = SeedIds.DesignUxTaskId,
                ProjectId = SeedIds.ClientWebsiteProjectId,
                Name = "Design & UX",
                Description = "Client-facing design work.",
                Classification = TaskClassification.CapEx,
                IsBillable = true,
                IsActive = true,
                CreatedAt = SeedIds.SeedCreatedAt
            },
            new
            {
                Id = SeedIds.ClientSupportTaskId,
                ProjectId = SeedIds.ClientWebsiteProjectId,
                Name = "Client Support",
                Description = "Ongoing client support and change requests.",
                Classification = TaskClassification.OpEx,
                IsBillable = true,
                IsActive = true,
                CreatedAt = SeedIds.SeedCreatedAt
            });
    }
}
