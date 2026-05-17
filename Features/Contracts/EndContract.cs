using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Contracts;

public class EndContract
{
    // 📥 Request DTO
    public class Request
    {
        public int ContractId { get; set; }
        public DateTime EndDate { get; set; } = DateTime.UtcNow;
    }

    // 📤 Response DTO
    public class Response
    {
        public int ContractId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
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
            // 🔥 1. Load contract + room
            var contract = await _context.Contracts
                .Include(c => c.Room)
                .FirstOrDefaultAsync(c => c.Id == request.ContractId);

            if (contract == null)
                throw new Exception("Hợp đồng không tồn tại");

            // 🔥 2. Check đã kết thúc chưa
            if (contract.Status == ContractStatus.Ended)
                throw new Exception("Hợp đồng đã kết thúc rồi");
            // 🔥 2. Check còn hóa đơn chưa thanh toán không
            if (contract.Bills.Any(b => b.Status == BillStatus.Unpaid))
                throw new Exception("Còn hóa đơn chưa thanh toán");

            // 🔥 3. Update contract
            contract.Status = ContractStatus.Ended;
            contract.EndDate = request.EndDate;

            // 🔥 4. Update room status
            contract.Room.Status = RoomStatus.Empty;

            // 🔥 5. Save DB
            await _context.SaveChangesAsync();

            // 🔥 6. Return
            return new Response
            {
                ContractId = contract.Id,
                RoomNumber = contract.Room.RoomNumber,
                Status = contract.Status.ToString()
            };
        }
    }
}
