using Microsoft.AspNetCore.Mvc;
using RoomManagement.WebAPI.Features.Identity;

namespace RoomManagement.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class IdentityController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] Login.Request request,
        [FromServices] Login.Handler handler)
    {
        var result = await handler.Handle(request);
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
    [FromBody] RegisterUser.Request request,
    [FromServices] RegisterUser.Handler handler)
    {
        var result = await handler.Handle(request);
        return Ok(result);
    }
}
