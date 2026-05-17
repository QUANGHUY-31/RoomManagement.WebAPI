using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Tenants;

public class GetTenants
{
    // 📤 Response DTO
    public class TenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }  
    }

    public class Handler
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TenantDto>> Handle()
        {
            return await _context.Tenants
                .AsNoTracking() // 🔥 tối ưu read-only
                 .OrderByDescending(t => t.Id)
                .Select(t => new TenantDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Phone = t.Phone,
                    CCCD = t.CCCD,
                    CreatedAt = t.CreatedAt,
                    RoomNumber = t.ContractTenants
                        .Where(ct => ct.Contract.Status == Common.Enums.ContractStatus.Active)
                        .Select(ct => ct.Contract.Room.RoomNumber)
                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();
        }
    }
}
