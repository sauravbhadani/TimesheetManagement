using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TimesheetManagement.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy", utc = DateTime.UtcNow });
}
