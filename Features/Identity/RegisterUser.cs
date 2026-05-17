using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Domain.Entities;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Identity;

public class RegisterUser
{
    // 📥 Request
    public class Request
    {
        public string Username { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    // 📤 Response
    public class Response
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;
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
            // 🔥 1. Check duplicate username
            var exists = await _context.Users
                .AnyAsync(x => x.Username == request.Username);

            if (exists)
                throw new Exception("Username đã tồn tại");

            // 🔥 2. Check tenant tồn tại bằng số điện thoại
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(x => x.Phone == request.Phone);

            if (tenant == null)
                throw new Exception("Số điện thoại chưa tồn tại trong hệ thống");

            // 🔥 3. Check tenant đã có tài khoản chưa
            if (tenant.UserId != null)
                throw new Exception("Số điện thoại đã đăng ký tài khoản");

            if (request.Username.Length < 3 || request.Username.Length > 20)
                throw new Exception("Username phải từ 3-20 ký tự");

            if (request.Username.Contains(" "))
                throw new Exception("Username không được chứa khoảng trắng");

            if (request.Password.Length < 6)
                throw new Exception("Mật khẩu tối thiểu 6 ký tự");

            // 🔥 4. Create user
            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Staff,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            // 🔥 5. Link tenant với user
            tenant.UserId = user.Id;

            await _context.SaveChangesAsync();

            // 🔥 6. Response
            return new Response
            {
                Id = user.Id,
                Username = user.Username
            };
        }
    }
}