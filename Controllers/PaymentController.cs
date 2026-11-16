using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoomManagement.WebAPI.Features.Payments;
using Microsoft.AspNetCore.WebUtilities;

namespace RoomManagement.WebAPI.Controllers;

[ApiController]
[Route("api/payment")]
[Authorize]
public class PaymentController : ControllerBase
{
	private readonly CreateVnPayPayment.Handler _createPayment;
	private readonly VnPayReturn.Handler _vnPayReturn;
	private readonly VnPayIpn.Handler _vnPayIpn;

	public PaymentController(
		CreateVnPayPayment.Handler createPayment,
		VnPayReturn.Handler vnPayReturn,
		VnPayIpn.Handler vnPayIpn)
	{
		_createPayment = createPayment;
		_vnPayReturn = vnPayReturn;
		_vnPayIpn = vnPayIpn;
	}

	[HttpPost("pay")]
	public async Task<IActionResult> PayBill([FromQuery] int billId)
	{
		var url = await _createPayment.Handle(
			new CreateVnPayPayment.Request { BillId = billId });

		return Ok(url);
	}

	[HttpGet("vnpay-ipn")]
	[AllowAnonymous]
	public async Task<IActionResult> VnPayIpn()
	{
		var result = await _vnPayIpn.Handle(Request.Query);
		return Ok(result);
	}

	[HttpGet("vnpay-return")]
	[AllowAnonymous]
	public async Task<IActionResult> VnPayReturn()
	{
		var result = await _vnPayReturn.Handle(Request.Query);
		var redirectUrl = QueryHelpers.AddQueryString(
			"http://localhost:5173/payment-result",
			new Dictionary<string, string?>
			{
				["isSuccess"] = result.IsSuccess.ToString().ToLowerInvariant(),
				["billId"] = result.BillId?.ToString(),
				["message"] = result.Message
			});

		return Redirect(redirectUrl);
	}
}