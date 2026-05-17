using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Dashboard;

public class GetDashboardSummary
{
    public class RevenuePointDto
    {
        public string Id { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class OccupancyPointDto
    {
        public string Id { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public decimal Rate { get; set; }
    }

    public class UnpaidBillDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string MonthYear { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class Response
    {
        public string LastUpdated { get; set; } = string.Empty;
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public decimal OccupancyRate { get; set; }
        public int TotalTenants { get; set; }
        public int ActiveContracts { get; set; }
        public int UnpaidBillsCount { get; set; }
        public decimal UnpaidBillsAmount { get; set; }
        public List<RevenuePointDto> RevenueData { get; set; } = new();
        public List<OccupancyPointDto> OccupancyData { get; set; } = new();
        public List<UnpaidBillDto> UnpaidBills { get; set; } = new();
    }

    public class Handler
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle()
        {
            var now = DateTime.UtcNow;
            var monthWindows = Enumerable.Range(0, 6)
                .Select(offset =>
                {
                    // Tính 
                    var date = new DateTime(now.Year, now.Month, 1).AddMonths(-(5 - offset));
                    var monthStart = date;
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                    return new
                    {
                        MonthStart = monthStart,
                        MonthEnd = monthEnd,
                        Month = $"T{monthStart.Month}",
                        Key = $"{monthStart.Year:D4}-{monthStart.Month:D2}"
                    };
                })
                .ToList();

            var totalRooms = await _context.Rooms.CountAsync();
            var occupiedRooms = await _context.Rooms.CountAsync(r => r.Status == RoomStatus.Occupied);
            var totalTenants = await _context.Tenants.CountAsync();
            var activeContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);

            var revenueLookup = await _context.Bills
                .AsNoTracking()
                .Where(b => monthWindows.Select(m => m.MonthStart.Year).Contains(b.Year))
                .GroupBy(b => new { b.Year, b.Month })
                .Select(group => new
                {
                    group.Key.Year,
                    group.Key.Month,
                    Revenue = group.Sum(b => b.Total)
                })
                .ToListAsync();

            var unpaidBillsQuery = _context.Bills
                .AsNoTracking()
                .Where(b => b.Status == BillStatus.Unpaid);

            var unpaidBillsCount = await unpaidBillsQuery.CountAsync();
            var unpaidBillsAmount = await unpaidBillsQuery.SumAsync(b => (decimal?)b.Total) ?? 0;

            var unpaidBills = await unpaidBillsQuery
                .Include(b => b.Contract)
                    .ThenInclude(c => c.Room)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ThenByDescending(b => b.Id)
                .Take(5)
                .Select(b => new UnpaidBillDto
                {
                    Id = b.Id,
                    RoomNumber = b.Contract.Room.RoomNumber,
                    MonthYear = $"{b.Month:D2}/{b.Year}",
                    Total = b.Total
                })
                .ToListAsync();

            var occupancyData = monthWindows
                .Select(window =>
                {
                    var occupiedInMonth = _context.Contracts.Count(c =>
                        c.StartDate <= window.MonthEnd &&
                        (c.EndDate == null || c.EndDate >= window.MonthStart));

                    var rate = totalRooms == 0
                        ? 0
                        : Math.Round((decimal)occupiedInMonth * 100 / totalRooms, 2);

                    return new OccupancyPointDto
                    {
                        Id = $"occ-{window.Key}",
                        Month = window.Month,
                        Rate = rate
                    };
                })
                .ToList();

            var revenueData = monthWindows
                .Select(window =>
                {
                    var revenue = revenueLookup
                        .FirstOrDefault(item =>
                            item.Year == window.MonthStart.Year &&
                            item.Month == window.MonthStart.Month)
                        ?.Revenue ?? 0;

                    return new RevenuePointDto
                    {
                        Id = $"rev-{window.Key}",
                        Month = window.Month,
                        Revenue = revenue
                    };
                })
                .ToList();

            var occupancyRate = totalRooms == 0
                ? 0
                : Math.Round((decimal)occupiedRooms * 100 / totalRooms, 2);

            return new Response
            {
                LastUpdated = now.ToString("dd/MM/yyyy HH:mm"),
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                OccupancyRate = occupancyRate,
                TotalTenants = totalTenants,
                ActiveContracts = activeContracts,
                UnpaidBillsCount = unpaidBillsCount,
                UnpaidBillsAmount = unpaidBillsAmount,
                RevenueData = revenueData,
                OccupancyData = occupancyData,
                UnpaidBills = unpaidBills
            };
        }
    }
}
