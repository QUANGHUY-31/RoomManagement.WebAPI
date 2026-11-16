using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomManagement.WebAPI.Features.Bills;
using System.Threading.Tasks;

namespace RoomManagement.WebAPI.Controllers;

[ApiController]
[Route("api/bills")]
[Authorize]
public class BillsController : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate(
        [FromBody] CalculateMonthlyBill.Request request,
        [FromServices] CalculateMonthlyBill.Handler handler)
    {
        var result = await handler.Handle(request);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("pay")]
    public async Task<IActionResult> Pay(
    [FromBody] MarkAsPaid.Request request,
    [FromServices] MarkAsPaid.Handler handler)
    {
        var result = await handler.Handle(request);
        return Ok(result);
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Get(
    [FromServices] GetBills.Handler handler)
    {
        var result = await handler.Handle();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateBill.Request request,
        [FromServices] UpdateBill.Handler handler)
    {
        var result = await handler.Handle(id, request);
        return Ok(result);
    }

    // 🔥 API cho tenant xem hóa đơn của mình
    [HttpGet("my-bills")]
    public async Task<IActionResult> GetMyBills(
    [FromServices] MyBill.Handler handler)
    {
        var result = await handler.Handle();

        return Ok(result);
    }
}
