using RoomManagement.WebAPI.Domain.Entities;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Tenants;

public class RegisterTenant
{
    // 📥 Request DTO
    public class Request
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
    }

    // 📤 Response DTO
    public class Response
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
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
            // 🔥 1. Validate
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new Exception("Tên không được để trống");

            if (string.IsNullOrWhiteSpace(request.Phone))
                throw new Exception("Số điện thoại không được để trống");

            if (string.IsNullOrWhiteSpace(request.CCCD))
                throw new Exception("CCCD không được để trống");

            // 🔥 2. kiểm tra  CCCD (quan trọng)
            var exists = _context.Tenants
                .Any(t => t.CCCD == request.CCCD);

            if (exists)
                throw new Exception("CCCD đã tồn tại");

            // 🔥 3. tạo entity
            var tenant = new Tenant
            {
                Name = request.Name,
                Phone = request.Phone,
                CCCD = request.CCCD,
                CreatedAt = DateTime.UtcNow
            };

            // 🔥 4. Save DB
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // 🔥 5. Return response
            return new Response
            {
                Id = tenant.Id,
                Name = tenant.Name
            };
        }
    }
}
