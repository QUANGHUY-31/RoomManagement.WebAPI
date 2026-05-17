using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RoomManagement.WebAPI.Features.Contracts;

public class MyContract
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

        private readonly IHttpContextAccessor _httpContextAccessor;

        public Handler(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response> Handle()
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

            // 🔥 Load active contract của tenant hiện tại
            var contract = await _context.Contracts
                .Include(c => c.Room)

                .Include(c => c.PrimaryTenant)

                .Include(c => c.ContractTenants)
                    .ThenInclude(ct => ct.Tenant)

                .FirstOrDefaultAsync(c =>
                    c.Status == ContractStatus.Active &&
                    c.ContractTenants.Any(ct =>
                        ct.Tenant.UserId == userId));

            if (contract == null)
            {
                throw new Exception("Không tìm thấy hợp đồng");
            }

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