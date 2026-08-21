using System.Security.Claims;
using TimesheetManagement.Application.Repositories;
using TimesheetManagement.Domain.Entities;
using TimesheetManagement.Domain.Enums;

namespace TimesheetManagement.Infrastructure.Auth;

/// <summary>
/// Maps an Entra ID token to the local Users table on first login (JIT provisioning), so the rest
/// of the app only ever deals with local User records regardless of which auth mode issued the token.
/// </summary>
public class EntraJitProvisioningService(IUserRepository userRepository, IUnitOfWork unitOfWork)
{
    public async Task<User> ProvisionOrGetAsync(ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var objectId = FirstValue(principal, "oid") ?? FirstValue(principal, ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Entra token is missing an object id (oid) claim.");

        var existing = await userRepository.GetByExternalAuthIdAsync(objectId, ct);
        if (existing is not null)
        {
            return existing;
        }

        var email = FirstValue(principal, ClaimTypes.Email) ?? FirstValue(principal, "preferred_username") ?? objectId;
        var name = FirstValue(principal, ClaimTypes.Name) ?? email;

        var user = new User
        {
            Id = Guid.NewGuid(),
            ExternalAuthId = objectId,
            FullName = name,
            Email = email,
            Role = ResolveRole(principal),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return user;
    }

    /// <summary>
    /// Reads Entra app-role / group claims and maps the first recognized one to our UserRole.
    /// New users default to Employee — an Admin promotes them via the Users screen afterwards.
    /// </summary>
    private static UserRole ResolveRole(ClaimsPrincipal principal)
    {
        var roleClaims = principal.FindAll("roles")
            .Concat(principal.FindAll(ClaimTypes.Role))
            .Select(c => c.Value);

        foreach (var claim in roleClaims)
        {
            if (Enum.TryParse<UserRole>(claim, ignoreCase: true, out var role))
            {
                return role;
            }
        }

        return UserRole.Employee;
    }

    private static string? FirstValue(ClaimsPrincipal principal, string claimType) =>
        principal.FindFirst(claimType)?.Value;
}
