using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Domain.Entities;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Contracts;

public class CreateContract
{
    // 📥 Request DTO
    public class Request
    {
        public int RoomId { get; set; }
        public int PrimaryTenantId { get; set; }
        public List<int> TenantIds { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    // 📤 Response DTO
    public class Response
    {
        public int ContractId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public int TotalTenants { get; set; }
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
            var tenantIds = request.TenantIds
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (tenantIds.Count == 0)
                throw new Exception("Hợp đồng phải có ít nhất 1 người thuê");

            if (!tenantIds.Contains(request.PrimaryTenantId))
                throw new Exception("Khách đứng tên hợp đồng phải nằm trong danh sách người thuê");

            // 🔥 1. Load Room
            var room = await _context.Rooms
                .Include(r => r.Contracts)
                .FirstOrDefaultAsync(r => r.Id == request.RoomId);

            if (room == null)
                throw new Exception("Phòng không tồn tại");

            // 🔥 2. Check room đang có hợp đồng active không
            var isOccupied = room.Contracts
                .Any(c => c.Status == ContractStatus.Active);

            if (isOccupied)
                throw new Exception("Phòng đang có người thuê");

            // 🔥 3. Check tenant tồn tại
            var tenants = await _context.Tenants
                .Where(t => tenantIds.Contains(t.Id))
                .ToListAsync();

            if (tenants.Count != tenantIds.Count)
                throw new Exception("Một số người thuê không tồn tại");

            var occupiedTenantNames = await _context.ContractTenants
                .Where(ct =>
                    tenantIds.Contains(ct.TenantId) &&
                    ct.Contract.Status == ContractStatus.Active)
                .Select(ct => ct.Tenant.Name)
                .Distinct()
                .ToListAsync();

            if (occupiedTenantNames.Any())
                throw new Exception(
                    $"Khách đã có hợp đồng đang hiệu lực: {string.Join(", ", occupiedTenantNames)}");

            // 🔥 4. Check primary tenant đứng tên hợp đồng
            var primaryTenant = tenants
                .FirstOrDefault(t => t.Id == request.PrimaryTenantId);

            if (primaryTenant == null)
                throw new Exception("Primary tenant không hợp lệ");

            // 🔥 5. Check sức chứa
            if (tenants.Count > room.MaxOccupancy)
                throw new Exception("Vượt quá sức chứa phòng");

            // 🔥 6. Create Contract
            var contract = new Contract
            {
                RoomId = room.Id,
                PrimaryTenantId = request.PrimaryTenantId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = ContractStatus.Active,
                ContractTenants = tenants.Select(t => new ContractTenant
                {
                    TenantId = t.Id
                }).ToList()
            };

            _context.Contracts.Add(contract);

            // 🔥 7. Update room status
            room.Status = RoomStatus.Occupied;

            // 🔥 8. Save DB
            await _context.SaveChangesAsync();

            // 🔥 9. Return
            return new Response
            {
                ContractId = contract.Id,
                RoomNumber = room.RoomNumber,
                TotalTenants = tenants.Count
            };
        }
    }
}
