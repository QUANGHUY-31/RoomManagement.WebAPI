using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Tenants;

public class UpdateTenant
{
    public class Request
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
    }

    public class Response
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

        public async Task<Response> Handle(int id, Request request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new Exception("Tên không được để trống");

            if (string.IsNullOrWhiteSpace(request.Phone))
                throw new Exception("Số điện thoại không được để trống");

            if (string.IsNullOrWhiteSpace(request.CCCD))
                throw new Exception("CCCD không được để trống");

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tenant == null)
                throw new Exception("Người thuê không tồn tại");

            var duplicateCccd = await _context.Tenants
                .AnyAsync(t => t.Id != id && t.CCCD == request.CCCD);

            if (duplicateCccd)
                throw new Exception("CCCD đã tồn tại");

            tenant.Name = request.Name;
            tenant.Phone = request.Phone;
            tenant.CCCD = request.CCCD;

            await _context.SaveChangesAsync();

            return new Response
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Phone = tenant.Phone,
                CCCD = tenant.CCCD
            };
        }
    }
}
