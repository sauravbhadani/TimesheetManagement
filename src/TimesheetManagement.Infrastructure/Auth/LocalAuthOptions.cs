namespace TimesheetManagement.Infrastructure.Auth;

/// <summary>
/// Bound from the "Auth:Local" config section. Only used when Auth:Provider = "Local".
/// SigningKey must come from user-secrets/environment in any shared environment — never checked in.
/// </summary>
public class LocalAuthOptions
{
    public const string SectionName = "Auth:Local";

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "TimesheetManagement.Local";
    public string Audience { get; set; } = "TimesheetManagement.Api";
    public int TokenLifetimeMinutes { get; set; } = 480;
}
