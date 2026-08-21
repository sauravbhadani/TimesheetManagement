using TimesheetManagement.Domain.Entities;

namespace TimesheetManagement.Application.Auth;

/// <summary>
/// Issues locally-signed JWTs for the dev login screen (Auth:Provider = "Local").
/// Not used at all when Auth:Provider = "EntraId" — tokens come from the tenant instead.
/// </summary>
public interface ILocalAuthTokenService
{
    string IssueToken(User user);
}
