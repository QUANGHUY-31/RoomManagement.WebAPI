using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Infrastructure;
using RoomManagement.WebAPI.Common.Enums;
using System.Security;

namespace RoomManagement.WebAPI.Features.Contracts;

public class GetContractDetail
{
    // 📤 Response DTO
    public class Response
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public RoomDto Room { get; set; } = null!;
        public TenantDto PrimaryTenant { get; set; } = null!;
        public List<TenantDto> Tenants { get; set; } = new();
    }

    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class TenantDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
    }

    public class Handler
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(int contractId)
        {
            // 🔥 1. Load contract + relations
            var contract = await _context.Contracts
                .Include(c => c.Room)
                .Include(c => c.PrimaryTenant)
                .Include(c => c.ContractTenants)
                    .ThenInclude(ct => ct.Tenant)
                .FirstOrDefaultAsync(c => c.Id == contractId);

            if (contract == null)
                throw new Exception("Hợp đồng không tồn tại");

            // 🔥 2. Map response
            return new Response
            {
                Id = contract.Id,
                Status = contract.Status.ToString(),
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,

                Room = new RoomDto
                {
                    Id = contract.Room.Id,
                    RoomNumber = contract.Room.RoomNumber,
                    Price = contract.Room.Price
                },

                PrimaryTenant = new TenantDto
                {
                    Id = contract.PrimaryTenant.Id,
                    Name = contract.PrimaryTenant.Name,
                    Phone = contract.PrimaryTenant.Phone,
                    CCCD = contract.PrimaryTenant.CCCD
                },

                Tenants = contract.ContractTenants
                    .Select(ct => new TenantDto
                    {
                        Id = ct.Tenant.Id,
                        Name = ct.Tenant.Name,
                        Phone = ct.Tenant.Phone,
                        CCCD = ct.Tenant.CCCD
                    })
                    .ToList()
            };
        }
    }
}
