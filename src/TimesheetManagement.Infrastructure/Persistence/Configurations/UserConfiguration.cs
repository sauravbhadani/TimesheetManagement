using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Domain.Enums;
using TimesheetManagement.Infrastructure.Persistence.Seed;

namespace TimesheetManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.ExternalAuthId).HasMaxLength(200).IsRequired();
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.ExternalAuthId).IsUnique();

        builder.HasOne(u => u.Manager)
            .WithMany(u => u.DirectReports)
            .HasForeignKey(u => u.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new User
            {
                Id = SeedIds.AdminUserId,
                ExternalAuthId = SeedIds.AdminUserId.ToString(),
                FullName = "Ava Admin",
                Email = "admin@timesheet.local",
                Role = UserRole.Admin,
                ManagerId = null,
                IsActive = true,
                CreatedAt = SeedIds.SeedCreatedAt
            },
            new User
            {
                Id = SeedIds.ManagerUserId,
                ExternalAuthId = SeedIds.ManagerUserId.ToString(),
                FullName = "Mia Manager",
                Email = "manager@timesheet.local",
                Role = UserRole.Manager,
                ManagerId = null,
                IsActive = true,
                CreatedAt = SeedIds.SeedCreatedAt
            },
            new User
            {
                Id = SeedIds.EmployeeUserId,
                ExternalAuthId = SeedIds.EmployeeUserId.ToString(),
                FullName = "Eli Employee",
                Email = "employee@timesheet.local",
                Role = UserRole.Employee,
                ManagerId = SeedIds.ManagerUserId,
                IsActive = true,
                CreatedAt = SeedIds.SeedCreatedAt
            });
    }
}
