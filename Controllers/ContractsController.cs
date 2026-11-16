using Microsoft.AspNetCore.Mvc;
using RoomManagement.WebAPI.Features.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace RoomManagement.WebAPI.Controllers;

[ApiController]
[Route("api/contracts")]
[Authorize]
public class ContractsController : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromServices] GetContracts.Handler handler)
    {
        var result = await handler.Handle();
        return Ok(result);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateContract.Request request,
        [FromServices] CreateContract.Handler handler)
    {
        var result = await handler.Handle(request);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("end")]
    public async Task<IActionResult> End(
    [FromBody] EndContract.Request request,
    [FromServices] EndContract.Handler handler)
    {
        var result = await handler.Handle(request);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/tenants")]
    public async Task<IActionResult> UpdateTenants(
        int id,
        [FromBody] UpdateContractTenants.Request request,
        [FromServices] UpdateContractTenants.Handler handler)
    {
        var result = await handler.Handle(id, request);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(
    int id,
    [FromServices] GetContractDetail.Handler handler)
    {
        var result = await handler.Handle(id);
        return Ok(result);
    }
    // 🔥 API cho tenant xem hợp đồng của mình
    [HttpGet("my-contract")]
    public async Task<IActionResult> GetMyContract(
    [FromServices] MyContract.Handler handler)
    {
        var result = await handler.Handle();

        return Ok(result);
    }
}
