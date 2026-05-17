using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Infrastructure;
using RoomManagement.WebAPI.Common.Enums;

namespace RoomManagement.WebAPI.Features.Rooms
{
    public class GetRooms
    {
        public class RoomDto
        {
            public int Id { get; set; }
            public string RoomNumber { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string Status { get; set; } = string.Empty;
            public int MaxOccupancy { get; set; }
            public string Description { get; set; } = string.Empty; // ✅ thêm
            public string? MainImage { get; set; }
        }

        public class Handler
        {
            private readonly AppDbContext _context;

            public Handler(AppDbContext context)
            {
                _context = context;
            }

            // ✅ Thêm tham số status
            public async Task<List<RoomDto>> Handle(string? status = null)
            {
                var query = _context.Rooms.AsQueryable();

                // ✅ Filter nếu có status
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<RoomStatus>(status, out var roomStatus))
                {
                    query = query.Where(r => r.Status == roomStatus);
                }

                return await query
                    .Select(r => new RoomDto
                    {
                        Id = r.Id,
                        RoomNumber = r.RoomNumber,
                        Price = r.Price,
                        Status = r.Status.ToString(),
                        MaxOccupancy = r.MaxOccupancy,
                        Description = r.Description, // ✅ thêm
                        MainImage = r.Images
                            .OrderByDescending(i => i.IsMain)
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault()
                    })
                    .ToListAsync();
            }
        }
    }
}