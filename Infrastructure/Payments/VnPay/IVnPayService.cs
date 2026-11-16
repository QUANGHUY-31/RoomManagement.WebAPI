using Microsoft.AspNetCore.Http;

namespace RoomManagement.WebAPI.Infrastructure.Payments.VnPay;

public interface IVnPayService
{
	string CreatePaymentUrl(HttpContext context, string txnRef, long amount);

	bool ValidateSignature(IQueryCollection query);

	string GetResponseData(IQueryCollection query, string key);
}