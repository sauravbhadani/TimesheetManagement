using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetManagement.Application.Dtos;
using TimesheetManagement.Application.Services;

namespace TimesheetManagement.Api.Controllers;

[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController(IUserService userService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll(CancellationToken ct) =>
        await userService.GetAllAsync(ct);

    [HttpPut("{id:guid}/role")]
    public async Task<ActionResult<UserDto>> UpdateRole(Guid id, UpdateUserRoleRequest request, CancellationToken ct) =>
        await userService.UpdateRoleAsync(id, request, ct);

    [HttpPut("{id:guid}/manager")]
    public async Task<ActionResult<UserDto>> UpdateManager(Guid id, UpdateUserManagerRequest request, CancellationToken ct) =>
        await userService.UpdateManagerAsync(id, request, ct);
}
