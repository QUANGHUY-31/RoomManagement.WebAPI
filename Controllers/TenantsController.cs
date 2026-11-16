using RoomManagement.WebAPI.Features.Tenants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace RoomManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/tenants")]
    [Authorize(Roles = "Admin")]
    public class  TennantsController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Register
            (
            [FromBody] RegisterTenant.Request request,
            [FromServices] RegisterTenant.Handler handler 
            )
        { 
            var result = await handler.Handle(request);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromServices] GetTenants.Handler handler)
        { var result = await handler.Handle();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateTenant.Request request,
            [FromServices] UpdateTenant.Handler handler)
        {
            var result = await handler.Handle(id, request);
            return Ok(result);
        }
        
    }
}
