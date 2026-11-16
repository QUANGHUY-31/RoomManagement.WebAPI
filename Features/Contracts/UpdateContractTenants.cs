using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Domain.Entities;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Contracts;

public class UpdateContractTenants
{
    public class Request
    {
        public List<int> TenantIds { get; set; } = new();
        public DateTime? EndDate { get; set; }
    }

    public class Response
    {
        public int ContractId { get; set; }
        public int TotalTenants { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class Handler
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(int id, Request request)
        {
            if (request.TenantIds == null || request.TenantIds.Any(tenantId => tenantId <= 0))
                throw new Exception("Danh sách người thuê không hợp lệ");

            var tenantIds = request.TenantIds.Distinct().ToList();
            if (tenantIds.Count == 0)
                throw new Exception("Hợp đồng phải có ít nhất 1 người thuê");

            var contract = await _context.Contracts
                .Include(c => c.Room)
                .Include(c => c.ContractTenants)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                throw new Exception("Hợp đồng không tồn tại");

            if (contract.Status != ContractStatus.Active)
                throw new Exception("Chỉ được sửa hợp đồng đang có hiệu lực");

            if (!tenantIds.Contains(contract.PrimaryTenantId))
                throw new Exception("Không thể xóa người đứng tên hợp đồng khỏi danh sách");

            if (tenantIds.Count > contract.Room.MaxOccupancy)
                throw new Exception("Vượt quá sức chứa phòng");

            if (request.EndDate.HasValue)
            {
                var requestedEndDate = request.EndDate.Value.Date;
                var currentEndDate = contract.EndDate?.Date ?? contract.StartDate.Date;
                if (requestedEndDate <= currentEndDate)
                    throw new Exception("Ngày kết thúc mới phải sau ngày kết thúc hiện tại");
            }

            var existingTenants = await _context.Tenants
                .Where(t => tenantIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            if (existingTenants.Count != tenantIds.Count)
                throw new Exception("Một số người thuê không tồn tại");

            var occupiedTenantIds = await _context.ContractTenants
                .Where(ct =>
                    tenantIds.Contains(ct.TenantId) &&
                    ct.ContractId != id &&
                    ct.Contract.Status == ContractStatus.Active)
                .Select(ct => ct.TenantId)
                .Distinct()
                .ToListAsync();

            var otherPrimaryTenantIds = await _context.Contracts
                .Where(c =>
                    tenantIds.Contains(c.PrimaryTenantId) &&
                    c.Id != id &&
                    c.Status == ContractStatus.Active)
                .Select(c => c.PrimaryTenantId)
                .Distinct()
                .ToListAsync();

            if (occupiedTenantIds.Union(otherPrimaryTenantIds).Any())
                throw new Exception("Một số người thuê đã có hợp đồng đang hiệu lực");

            var requestedTenantIdSet = tenantIds.ToHashSet();
            var tenantsToRemove = contract.ContractTenants
                .Where(ct => !requestedTenantIdSet.Contains(ct.TenantId))
                .ToList();
            _context.ContractTenants.RemoveRange(tenantsToRemove);

            var existingTenantIdSet = contract.ContractTenants
                .Select(ct => ct.TenantId)
                .ToHashSet();
            foreach (var tenantId in tenantIds.Where(tenantId => !existingTenantIdSet.Contains(tenantId)))
            {
                contract.ContractTenants.Add(new ContractTenant
                {
                    TenantId = tenantId
                });
            }

            if (request.EndDate.HasValue)
                contract.EndDate = request.EndDate.Value;

            await _context.SaveChangesAsync();

            return new Response
            {
                ContractId = contract.Id,
                TotalTenants = tenantIds.Count,
                EndDate = contract.EndDate
            };
        }
    }
}
