using System.Globalization;
using System.Data;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;
using RoomManagement.WebAPI.Infrastructure.Payments.VnPay;

namespace RoomManagement.WebAPI.Features.Payments;

public class VnPayIpn
{
    public class Response
    {
        [JsonPropertyName("RspCode")]
        public string RspCode { get; set; } = string.Empty;

        [JsonPropertyName("Message")]
        public string Message { get; set; } = string.Empty;
    }

    public class Handler
    {
        private readonly AppDbContext _context;
        private readonly IVnPayService _vnPay;

        public Handler(AppDbContext context, IVnPayService vnPay)
        {
            _context = context;
            _vnPay = vnPay;
        }

        public async Task<Response> Handle(IQueryCollection query)
        {
            if (!_vnPay.ValidateSignature(query))
                return CreateResponse("97", "Invalid Checksum");

            var txnRef = query["vnp_TxnRef"].ToString();
            if (string.IsNullOrWhiteSpace(txnRef))
                return CreateResponse("01", "Order not found");

            await using var dbTransaction =
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var transaction = await _context.PaymentTransactions
                .Include(payment => payment.Bill)
                .FirstOrDefaultAsync(payment => payment.TxnRef == txnRef);

            if (transaction == null)
                return CreateResponse("01", "Order not found");

            if (!long.TryParse(
                    query["vnp_Amount"].ToString(),
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var receivedAmount) ||
                transaction.Amount != decimal.Truncate(transaction.Amount) ||
                (decimal)receivedAmount != transaction.Amount * 100m)
            {
                return CreateResponse("04", "Invalid amount");
            }

            if (transaction.Status != PaymentStatus.Pending)
                return CreateResponse("02", "Order already confirmed");

            var responseCode = query["vnp_ResponseCode"].ToString();
            var transactionStatus = query["vnp_TransactionStatus"].ToString();
            transaction.CompletedAt = DateTime.UtcNow;

            if (responseCode == "00" && transactionStatus == "00")
            {
                transaction.Status = PaymentStatus.Success;
                transaction.VnPayTranNo = query["vnp_TransactionNo"].ToString();
                transaction.BankCode = query["vnp_BankCode"].ToString();

                if (transaction.Bill.Status == BillStatus.Unpaid)
                {
                    transaction.Bill.Status = BillStatus.Paid;
                    transaction.Bill.PaidAt = transaction.CompletedAt;
                }
            }
            else
            {
                transaction.Status = PaymentStatus.Failed;
            }

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
            return CreateResponse("00", "Confirm Success");
        }

        private static Response CreateResponse(string code, string message)
        {
            return new Response
            {
                RspCode = code,
                Message = message
            };
        }
    }
}
