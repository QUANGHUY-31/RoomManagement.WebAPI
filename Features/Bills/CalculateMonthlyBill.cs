using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Domain.Entities;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Bills;

public class CalculateMonthlyBill
{
    // 📥 Request DTO
    public class Request
    {
        public int ContractId { get; set; }

        public int ElectricityOld { get; set; }
        public int ElectricityNew { get; set; }

        public decimal ElectricityPrice { get; set; }
        public decimal WaterPrice { get; set; }
        public decimal ServicePrice { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }
    }

    // 📤 Response DTO
    public class Response
    {
        public int BillId { get; set; }
        public decimal Total { get; set; }
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
            // 🔥 1. Check contract
            var contract = await _context.Contracts
                .Include(c => c.Room)
                .Include(c => c.Bills)
                .FirstOrDefaultAsync(c => c.Id == request.ContractId);

            if (contract == null)
                throw new Exception("Hợp đồng không tồn tại");

            // 🔥 2. Check duplicate bill (1 tháng chỉ 1 bill)
            var exists = contract.Bills.Any(b =>
                b.Month == request.Month &&
                b.Year == request.Year);

            if (exists)
                throw new Exception("Hóa đơn tháng này đã tồn tại");

            // 🔥 3. Validate điện
            if (request.ElectricityNew < request.ElectricityOld)
                throw new Exception("Chỉ số điện không hợp lệ");

            // 🔥 4. Tính tiền
            var electricityUsed = request.ElectricityNew - request.ElectricityOld;
            var roomPrice = contract.Room.Price;

            var total =
                (electricityUsed * request.ElectricityPrice) +
                request.WaterPrice +
                request.ServicePrice+
                roomPrice;

            // 🔥 5. Create bill
            var bill = new Bill
            {
                ContractId = contract.Id,
                ElectricityOld = request.ElectricityOld,
                ElectricityNew = request.ElectricityNew,
                ElectricityPrice = request.ElectricityPrice,
                WaterPrice = request.WaterPrice,
                ServicePrice = request.ServicePrice,
                Price = roomPrice,
                Total = total,
                Status = BillStatus.Unpaid,
                CreatedAt = DateTime.UtcNow,

                Month = request.Month,
                Year = request.Year
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            // 🔥 6. Response
            return new Response
            {
                BillId = bill.Id,
                Total = bill.Total,
                Status = bill.Status.ToString()
            };
        }
    }
}
