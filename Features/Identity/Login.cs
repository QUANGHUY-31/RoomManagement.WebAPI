using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoomManagement.WebAPI.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace RoomManagement.WebAPI.Features.Identity;

public class Login
{
    // 📥 Request
    public class Request
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // 📤 Response
    public class Response
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class Handler
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public Handler(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<Response> Handle(Request request)
        {
            // 🔥 1. Kiểm tra người dùng 
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                throw new Exception("Sai tài khoản hoặc mật khẩu");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Sai tài khoản hoặc mật khẩu");

            // 🔥 2. Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                new Claim(ClaimTypes.Name, user.Username),
                    // 🔥 FIX Ở ĐÂY
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            // 🔥 3. JWT key
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // 🔥 4. Return
            return new Response
            {
                Token = jwt,
                Role = user.Role.ToString()
            };
        }
    }
}
