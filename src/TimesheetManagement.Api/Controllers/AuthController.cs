using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetManagement.Api.Dtos;
using TimesheetManagement.Application.Auth;
using TimesheetManagement.Application.Exceptions;
using TimesheetManagement.Application.Repositories;

namespace TimesheetManagement.Api.Controllers;

/// <summary>
/// Local mode: dev login surface (dropdown of seeded users -> local JWT).
/// EntraId mode: MSAL talks to the tenant directly, this controller only serves "me".
/// Either way, the client hydrates its session by calling "me" once it has a token, so the
/// frontend never has to know which mode resolved its role.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(
    IUserRepository userRepository,
    ILocalAuthTokenService tokenService,
    ICurrentUserService currentUser,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("local-users")]
    [AllowAnonymous]
    public async Task<ActionResult<List<LocalUserOptionDto>>> GetLocalUsers(CancellationToken ct)
    {
        if (!IsLocalModeActive())
        {
            return NotFound();
        }

        var users = await userRepository.GetAllAsync(ct);
        return users
            .Where(u => u.IsActive)
            .Select(u => new LocalUserOptionDto(u.Id, u.FullName, u.Email, u.Role.ToString()))
            .ToList();
    }

    [HttpPost("local-login")]
    [AllowAnonymous]
    public async Task<ActionResult<LocalLoginResponse>> LocalLogin(LocalLoginRequest request, CancellationToken ct)
    {
        if (!IsLocalModeActive())
        {
            return NotFound();
        }

        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null || !user.IsActive)
        {
            return Unauthorized();
        }

        var token = tokenService.IssueToken(user);
        var dto = new LocalUserOptionDto(user.Id, user.FullName, user.Email, user.Role.ToString());
        return new LocalLoginResponse(token, dto);
    }

    /// <summary>Resolves the caller's own profile from whichever token they presented. Used by the SPA on startup.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(currentUser.UserId, ct)
            ?? throw new NotFoundException("Current user was not found.");

        return new CurrentUserDto(user.Id, user.FullName, user.Email, user.Role.ToString());
    }

    private bool IsLocalModeActive() =>
        string.Equals(configuration["Auth:Provider"], "Local", StringComparison.OrdinalIgnoreCase);
}
