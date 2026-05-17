using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace RoomManagement.WebAPI.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
	private readonly CreateVnPayPayment.Handler _createPayment;
	private readonly VnPayReturn.Handler _vnPayReturn;

	public PaymentController(
		CreateVnPayPayment.Handler createPayment,
		VnPayReturn.Handler vnPayReturn)
	{
		_createPayment = createPayment;
		_vnPayReturn = vnPayReturn;
	}

	[HttpPost("pay")]
	public IActionResult PayBill(int billId)
	{
		var url = _createPayment.Handle(
			new CreateVnPayPayment.Request { BillId = billId },
			HttpContext
		);

		return Ok(url);
	}

	[HttpGet("vnpay-return")]
	public async Task<IActionResult> VnPayReturn()
	{
		await _vnPayReturn.Handle(Request.Query);

		return Redirect("http://localhost:5173/payment-result");
	}
}