namespace TimesheetManagement.Infrastructure.Auth;

/// <summary>
/// Bound from "Auth:EntraId". Only used when Auth:Provider = "EntraId" — every value here is
/// customer-specific and lives in that customer's config/environment, never in source.
/// </summary>
public class EntraIdOptions
{
    public const string SectionName = "Auth:EntraId";

    public string Instance { get; set; } = "https://login.microsoftonline.com/";
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Expected audience on inbound tokens; defaults to api://{ClientId} if left blank.</summary>
    public string Audience { get; set; } = string.Empty;
}
