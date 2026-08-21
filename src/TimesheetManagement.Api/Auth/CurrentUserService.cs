using System.Security.Claims;
using TimesheetManagement.Application.Auth;
using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Api.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid UserId =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("No authenticated user id on the current request.");

    public UserRole Role =>
        Enum.TryParse<UserRole>(Principal?.FindFirstValue(ClaimTypes.Role), ignoreCase: true, out var role)
            ? role
            : throw new InvalidOperationException("No authenticated user role on the current request.");
}
