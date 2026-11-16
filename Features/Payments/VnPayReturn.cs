using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;
using RoomManagement.WebAPI.Infrastructure.Payments.VnPay;

namespace RoomManagement.WebAPI.Features.Payments;

public class VnPayReturn
{
    public class Response
    {
        public bool IsSuccess { get; set; }
        public int? BillId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class Handler
    {
        private readonly IVnPayService _vnPay;
        private readonly AppDbContext _context;

        public Handler(IVnPayService vnPay, AppDbContext context)
        {
            _vnPay = vnPay;
            _context = context;
        }

        public async Task<Response> Handle(IQueryCollection query)
        {
            if (!_vnPay.ValidateSignature(query))
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Kết quả thanh toán không hợp lệ (Sai chữ ký)"
                };
            }

            var txnRef = query["vnp_TxnRef"].ToString();
            if (string.IsNullOrWhiteSpace(txnRef))
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy mã tham chiếu giao dịch"
                };
            }

            await using var dbTransaction =
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var transaction = await _context.PaymentTransactions
                .Include(payment => payment.Bill)
                .FirstOrDefaultAsync(payment => payment.TxnRef == txnRef);

            if (transaction == null)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy giao dịch thanh toán"
                };
            }

            var responseCode = query["vnp_ResponseCode"].ToString();
            var transactionStatus = query["vnp_TransactionStatus"].ToString();
            var isSuccess = responseCode == "00" && transactionStatus == "00";

            // Nếu giao dịch vẫn đang Pending, tiến hành cập nhật trạng thái
            if (transaction.Status == PaymentStatus.Pending)
            {
                transaction.CompletedAt = DateTime.UtcNow;

                if (isSuccess)
                {
                    if (long.TryParse(
                            query["vnp_Amount"].ToString(),
                            NumberStyles.None,
                            CultureInfo.InvariantCulture,
                            out var receivedAmount) &&
                        transaction.Amount == decimal.Truncate(transaction.Amount) &&
                        (decimal)receivedAmount == transaction.Amount * 100m)
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
                        isSuccess = false;
                    }
                }
                else
                {
                    transaction.Status = PaymentStatus.Failed;
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            else if (transaction.Status == PaymentStatus.Success)
            {
                // Nếu đã được cập nhật trước đó (ví dụ bởi IPN)
                isSuccess = true;
            }

            return new Response
            {
                IsSuccess = isSuccess,
                BillId = transaction.BillId,
                Message = isSuccess
                    ? "Giao dịch thanh toán thành công"
                    : "Giao dịch không thành công"
            };
        }
    }
}
