using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Rooms;

public class UpdateRoom
{
    public class Request
    {
        public decimal Price { get; set; }
        public int MaxOccupancy { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class Response
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxOccupancy { get; set; }
        public string Description { get; set; } = string.Empty;
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
            if (request.Price <= 0)
                throw new Exception("Giá phòng phải lớn hơn 0");

            if (request.MaxOccupancy <= 0)
                throw new Exception("Sức chứa phòng phải lớn hơn 0");

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
                throw new Exception("Phòng không tồn tại");

            var currentTenantCount = await _context.ContractTenants
                .CountAsync(ct =>
                    ct.Contract.RoomId == id &&
                    ct.Contract.Status == ContractStatus.Active);

            if (request.MaxOccupancy < currentTenantCount)
                throw new Exception("Sức chứa mới nhỏ hơn số người đang thuê");

            room.Price = request.Price;
            room.MaxOccupancy = request.MaxOccupancy;
            room.Description = request.Description;

            await _context.SaveChangesAsync();

            return new Response
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                Price = room.Price,
                MaxOccupancy = room.MaxOccupancy,
                Description = room.Description
            };
        }
    }
}
