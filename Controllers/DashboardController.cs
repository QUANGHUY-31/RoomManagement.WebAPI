using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomManagement.WebAPI.Features.Dashboard;

namespace RoomManagement.WebAPI.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromServices] GetDashboardSummary.Handler handler,
        [FromQuery] int year = 0,
        [FromQuery] int? month = null
        )
    {
        var result = await handler.Handle();
        return Ok(result);
    }
}
