using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RoomManagement.WebAPI.Features.Rooms;

public class MyRoom
{
    public class Response
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public int MaxOccupancy { get; set; }
        public string Description { get; set; } = string.Empty;

        public string? MainImage { get; set; }

        public List<string> Images { get; set; } = new();

        public TenantDto? PrimaryTenant { get; set; }

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

            var contract = await _context.Contracts
                .Include(c => c.Room)
                    .ThenInclude(r => r.Images)

                .Include(c => c.PrimaryTenant)

                .Include(c => c.ContractTenants)
                    .ThenInclude(ct => ct.Tenant)

                .Include(c => c.Bills)

                .FirstOrDefaultAsync(c =>
                    c.Status == ContractStatus.Active &&
                    c.ContractTenants.Any(ct =>
                        ct.Tenant.UserId == userId));

            if (contract == null || contract.Room == null)
            {
                throw new Exception("Chưa có phòng");
            }

            var room = contract.Room;

            return new Response
            {
                Id = room.Id,

                RoomNumber = room.RoomNumber,

                Price = room.Price,

                Status = room.Status.ToString(),

                MaxOccupancy = room.MaxOccupancy,

                Description = room.Description,

                MainImage = room.Images
                    .FirstOrDefault(i => i.IsMain)
                    ?.ImageUrl,

                Images = room.Images
                    .Select(i => i.ImageUrl)
                    .ToList(),

                ContractStartDate = contract.StartDate,

                ContractEndDate = contract.EndDate,

                ActiveContractId = contract.Id,

                PrimaryTenant = contract.PrimaryTenant == null
                    ? null
                    : new TenantDto
                    {
                        Id = contract.PrimaryTenant.Id,
                        Name = contract.PrimaryTenant.Name,
                        Phone = contract.PrimaryTenant.Phone
                    },

                Tenants = contract.ContractTenants
                    .Select(ct => new TenantDto
                    {
                        Id = ct.Tenant.Id,
                        Name = ct.Tenant.Name,
                        Phone = ct.Tenant.Phone
                    })
                    .ToList(),

                Bills = contract.Bills
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