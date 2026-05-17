using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RoomManagement.WebAPI.Infrastructure.Payments.VnPay;
using RoomManagement.WebAPI.Features.Bills;

public class VnPayReturn
{
	public class Handler
	{
		private readonly IVnPayService _vnPay;
		private readonly MarkAsPaid.Handler _markAsPaid;

		public Handler(
			IVnPayService vnPay,
			MarkAsPaid.Handler markAsPaid
		)
		{
			_vnPay = vnPay;
			_markAsPaid = markAsPaid;
		}

		public async Task Handle(IQueryCollection query)
		{
			// 🔐 1. Validate chữ ký
			if (!_vnPay.ValidateSignature(query))
				throw new Exception("Invalid signature");

			var billId = int.Parse(query["vnp_TxnRef"]);
			var responseCode = query["vnp_ResponseCode"];

			// ✅ 2. Thành công → gọi lại feature Bill
			if (responseCode == "00")
			{
				await _markAsPaid.Handle(new MarkAsPaid.Request
				{
					BillId = billId
				});
			}
		}
	}
}