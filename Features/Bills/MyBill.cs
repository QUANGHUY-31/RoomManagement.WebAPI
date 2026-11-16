using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RoomManagement.WebAPI.Features.Bills;

public class MyBill
{
    // 📤 DTO
    public class BillDto
    {
        public int Id { get; set; }

        public int ContractId { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public decimal Price { get; set; }

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

        private readonly IHttpContextAccessor _httpContextAccessor;

        public Handler(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<BillDto>> Handle()
        {
            var userIdClaim = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new Exception("Unauthorized");
            }

            var userId = int.Parse(userIdClaim);

            return await _context.Bills
                .AsNoTracking()

                .Include(b => b.Contract)
                    .ThenInclude(c => c.Room)

                .Include(b => b.Contract)
                    .ThenInclude(c => c.ContractTenants)
                        .ThenInclude(ct => ct.Tenant)

                .Where(b =>
                    b.Contract != null &&
                    b.Contract.ContractTenants.Any(ct =>
                        ct.Tenant.UserId == userId))

                .OrderByDescending(b => b.Id)

                .Select(b => new BillDto
                {
                    Id = b.Id,

                    ContractId = b.ContractId,

                    RoomNumber =
                        b.Contract != null &&
                        b.Contract.Room != null
                            ? b.Contract.Room.RoomNumber
                            : "Không có phòng",

                    Price = b.Price,

                    ElectricityOld = b.ElectricityOld,

                    ElectricityNew = b.ElectricityNew,

                    ElectricityPrice = b.ElectricityPrice,

                    WaterPrice = b.WaterPrice,

                    ServicePrice = b.ServicePrice,

                    Total = b.Total,

                    Status = b.Status.ToString(),

                    CreatedAt = b.CreatedAt,

                    PaidAt =
                        b.Status == BillStatus.Paid
                            ? b.PaidAt
                            : null,

                    Month = b.Month,

                    Year = b.Year
                })

                .ToListAsync();
        }
    }
}