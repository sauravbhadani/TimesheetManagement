namespace TimesheetManagement.Infrastructure.Persistence.Seed;

/// <summary>Fixed GUIDs for seed data so migrations produce deterministic HasData rows.</summary>
public static class SeedIds
{
    public static readonly Guid AdminUserId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ManagerUserId = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid EmployeeUserId = new("33333333-3333-3333-3333-333333333333");

    public static readonly Guid InternalToolsProjectId = new("aaaaaaaa-0000-0000-0000-000000000001");
    public static readonly Guid ClientWebsiteProjectId = new("aaaaaaaa-0000-0000-0000-000000000002");

    public static readonly Guid PlatformMaintenanceTaskId = new("bbbbbbbb-0000-0000-0000-000000000001");
    public static readonly Guid NewFeatureDevTaskId = new("bbbbbbbb-0000-0000-0000-000000000002");
    public static readonly Guid DesignUxTaskId = new("bbbbbbbb-0000-0000-0000-000000000003");
    public static readonly Guid ClientSupportTaskId = new("bbbbbbbb-0000-0000-0000-000000000004");

    public static readonly DateTime SeedCreatedAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
