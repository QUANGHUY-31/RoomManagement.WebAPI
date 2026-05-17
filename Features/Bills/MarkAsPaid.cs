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
            // 🔥 1. Load bill
            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.Id == request.BillId);

            if (bill == null)
                throw new Exception("Hóa đơn không tồn tại");

            // 🔥 3. Update status
            bill.Status = BillStatus.Paid;

            // Cập nhật thời gian thanh toán
            bill.PaidAt = DateTime.UtcNow;

            // 🔥 4. Save DB
            await _context.SaveChangesAsync();

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
