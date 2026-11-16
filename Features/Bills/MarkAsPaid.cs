using System.Data;
using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Bills;

public class MarkAsPaid
{
    // 📥 Request DTO
    public class Request
    {
        public int BillId { get; set; }
    }

    // 📤 Response DTO
    public class Response
    {
        public int BillId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class Handler
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(Request request)
        {
            await using var dbTransaction =
                await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            // 🔥 1. Load bill
            var bill = await _context.Bills
                .Include(b => b.PaymentTransactions)
                .FirstOrDefaultAsync(b => b.Id == request.BillId);

            if (bill == null)
                throw new Exception("Hóa đơn không tồn tại");

            if (bill.Status == BillStatus.Paid)
                throw new Exception("Hóa đơn đã được thanh toán");

            var now = DateTime.UtcNow;
            var cutoff = now.AddMinutes(-15);
            var pendingPayments = bill.PaymentTransactions
                .Where(payment => payment.Status == PaymentStatus.Pending)
                .ToList();
            var expiredPayments = pendingPayments
                .Where(payment => payment.CreatedAt <= cutoff)
                .ToList();

            foreach (var payment in expiredPayments)
            {
                payment.Status = PaymentStatus.Expired;
                payment.CompletedAt = now;
            }

            if (pendingPayments.Any(payment => payment.CreatedAt > cutoff))
            {
                if (expiredPayments.Count > 0)
                {
                    await _context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                }

                throw new Exception(
                    "Hóa đơn đang có phiên thanh toán VNPay đang chờ xử lý, vui lòng chờ sau 15 phút hoặc kiểm tra lại với khách");
            }

            // 🔥 3. Update status
            bill.Status = BillStatus.Paid;

            // Cập nhật thời gian thanh toán
            bill.PaidAt = now;

            // 🔥 4. Save DB
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            // 🔥 5. Response
            if (bill.Status == BillStatus.Paid)
            {
                return new Response
                {
                    BillId = bill.Id,
                    Status = bill.Status.ToString(),
                    Total = bill.Total
                };
            }

            throw new Exception("Không thể cập nhật trạng thái hóa đơn");
        }
    }
}
