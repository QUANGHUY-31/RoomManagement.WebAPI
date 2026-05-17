using RoomManagement.WebAPI.Domain.Entities;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Rooms;

public class CreateRoom
{
    // 📥 Request DTO
    public class Request
    {
        public string RoomNumber { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxOccupancy { get; set; }
        public string Description { get; set; } = string.Empty;

        public List<string>? ImageUrls { get; set; }
    }

    // 📤 Response DTO
    public class Response
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
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
            if (string.IsNullOrWhiteSpace(request.RoomNumber))
                throw new Exception("Số phòng là bắt buộc");

            if (request.Price <= 0)
                throw new Exception("Giá tiền phải lớn hơn 0");

            if (request.MaxOccupancy <= 0)
                throw new Exception("Số lượng người tối đa phải lớn hơn 0");

            // 🔥 2. Check duplicate RoomNumber
            var exists = _context.Rooms
                .Any(r => r.RoomNumber == request.RoomNumber);

            if (exists)
                throw new Exception("Số phòng đã tồn tại");

            // 🔥 3. Tạo Room
            var room = new Room
            {
                RoomNumber = request.RoomNumber,
                Price = request.Price,
                MaxOccupancy = request.MaxOccupancy,
                Description = request.Description
            };

            // 🔥 4. Thêm ảnh
            if (request.ImageUrls != null && request.ImageUrls.Any())
            {
                room.Images = request.ImageUrls
                    .Select((url, index) => new RoomImage
                    {
                        ImageUrl = url,
                        IsMain = index == 0 // ảnh đầu tiên là ảnh chính
                    })
                    .ToList();
            }

            // 🔥 5. Save DB
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            // 🔥 6. Return
            return new Response
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber
            };
        }
    }
}
