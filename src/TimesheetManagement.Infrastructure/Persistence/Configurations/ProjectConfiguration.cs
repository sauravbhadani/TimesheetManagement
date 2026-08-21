using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Infrastructure.Persistence.Seed;

namespace TimesheetManagement.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Code).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000);

        builder.HasIndex(p => p.Code).IsUnique();

        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new
            {
                Id = SeedIds.InternalToolsProjectId,
                Name = "Internal Tools",
                Code = "INT-001",
                Description = "Internal tooling and platform work.",
                IsActive = true,
                CreatedBy = SeedIds.AdminUserId,
                CreatedAt = SeedIds.SeedCreatedAt
            },
            new
            {
                Id = SeedIds.ClientWebsiteProjectId,
                Name = "Client Website Revamp",
                Code = "CWR-100",
                Description = "External client website redesign.",
                IsActive = true,
                CreatedBy = SeedIds.AdminUserId,
                CreatedAt = SeedIds.SeedCreatedAt
            });
    }
}
