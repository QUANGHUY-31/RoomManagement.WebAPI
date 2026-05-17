using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Rooms;

public class GetRoomDetail
{
    public class Response
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public int MaxOccupancy { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new();
        public TenantDto? PrimaryTenant { get; set; }
        // Response DTO — thêm field này
        public int? ActiveContractId { get; set; }

        public List<TenantDto> Tenants { get; set; } = new();
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public List<BillDto> Bills { get; set; } = new();
    }

    public class TenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class BillDto
    {
        public int Id { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
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

        public async Task<Response> Handle(int roomId)
        {
            var room = await _context.Rooms
                .Include(r => r.Images)
                .Include(r => r.Contracts)
                    .ThenInclude(c => c.PrimaryTenant)
                .Include(r => r.Contracts)
                    .ThenInclude(c => c.ContractTenants)
                        .ThenInclude(ct => ct.Tenant)
                .Include(r => r.Contracts)
                    .ThenInclude(c => c.Bills)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
                throw new Exception("Phòng không tồn tại");

            return new Response
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                Price = room.Price,
                Status = room.Status.ToString(),
                MaxOccupancy = room.MaxOccupancy,
                Description = room.Description,

                Images = room.Images
                    .Select(i => i.ImageUrl)
                    .ToList(),

                // ✅ Tất cả đều đi qua room.Contracts
                ContractStartDate = room.Contracts
                    .Where(c => c.Status == ContractStatus.Active)
                    .Select(c => c.StartDate)
                    .FirstOrDefault(),

                ContractEndDate = room.Contracts
                    .Where(c => c.Status == ContractStatus.Active)
                    .Select(c => (DateTime?)c.EndDate)
                    .FirstOrDefault(),
                // Handler — map từ room.Contracts
                ActiveContractId = room.Contracts
                     .Where(c => c.Status == ContractStatus.Active)
                     .Select(c => (int?)c.Id)  // ← c.Id là Id của Contract, không phải ContractTenant
                     .FirstOrDefault(),

                PrimaryTenant = room.Contracts
                    .Where(c => c.Status == ContractStatus.Active)
                    .Select(c => new TenantDto
                    {
                        Id = c.PrimaryTenant.Id,
                        Name = c.PrimaryTenant.Name,
                        Phone = c.PrimaryTenant.Phone
                    })
                    .FirstOrDefault(),

                Tenants = room.Contracts
                    .Where(c => c.Status == ContractStatus.Active)
                    .SelectMany(c => c.ContractTenants)
                    .Select(ct => new TenantDto
                    {
                        Id = ct.Tenant.Id,
                        Name = ct.Tenant.Name,
                        Phone = ct.Tenant.Phone
                    })
                    .ToList(),

                Bills = room.Contracts
                    .SelectMany(c => c.Bills)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new BillDto
                    {
                        Id = b.Id,
                        Total = b.Total,
                        Status = b.Status.ToString(),
                        CreatedAt = b.CreatedAt,
                        Month = b.Month,
                        Year = b.Year
                    })
                    .ToList()
            };
        }
    }
}