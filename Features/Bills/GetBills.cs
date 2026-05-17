using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Bills;

public class GetBills
{
    // 📤 DTO
    public class BillDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public decimal Price {  get; set; }
        public int ElectricityOld { get; set; }
        public int ElectricityNew { get; set; }
        public decimal ElectricityPrice { get; set; }
        public decimal WaterPrice { get; set; }
        public decimal ServicePrice { get; set; }

        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class Handler
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BillDto>> Handle()
        {
            return await _context.Bills
                .AsNoTracking()
                .Include(b => b.Contract)
                    .ThenInclude(c => c.Room)
                .OrderByDescending(b => b.Id)
                .Select(b => new BillDto
                {
                    Id = b.Id,
                    ContractId = b.ContractId,
                    RoomNumber = b.Contract != null && b.Contract.Room != null
                    ? b.Contract.Room.RoomNumber
                    : "Không có phòng",

                    Price = b.Contract != null && b.Contract.Room != null
                    ? b.Contract.Room.Price
                    : 0,// ✅ Thêm
                    ElectricityOld = b.ElectricityOld,        // ✅ Thêm
                    ElectricityNew = b.ElectricityNew,        // ✅ Thêm
                    ElectricityPrice = b.ElectricityPrice,    // ✅ Thêm
                    WaterPrice = b.WaterPrice,                // ✅ Thêm
                    ServicePrice = b.ServicePrice,

                    Total = b.Total,
                    Status = b.Status.ToString(),

                    CreatedAt = b.CreatedAt,
                    PaidAt = b.Status == BillStatus.Paid ? b.PaidAt : null,

                    Month = b.Month,
                    Year = b.Year
                })
                .ToListAsync();
        }
    }
}
