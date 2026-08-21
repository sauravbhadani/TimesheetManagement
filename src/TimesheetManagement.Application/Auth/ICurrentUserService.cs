using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Application.Auth;

/// <summary>
/// Resolves the caller's identity from whichever auth scheme handled the request
/// (Local or EntraId). Services depend on this, never on HttpContext directly, so
/// business logic stays oblivious to which auth mode is active.
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    UserRole Role { get; }
}
