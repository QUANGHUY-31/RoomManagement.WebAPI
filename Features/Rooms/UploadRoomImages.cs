using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Domain.Entities;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Rooms;

public class UploadRoomImages
{
    public class Request
    {
        public int RoomId { get; set; }
        public List<IFormFile> Files { get; set; } = new();
    }

    public class Response
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
    }

    public class Handler
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public Handler(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<List<Response>> Handle(Request request)
        {
            var room = await _db.Rooms.FindAsync(request.RoomId);
            if (room == null)
                throw new Exception("Room not found");

            var result = new List<Response>();

            foreach (var file in request.Files)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var path = Path.Combine(_env.WebRootPath, "images/rooms", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                var url = $"/images/rooms/{fileName}";

                var image = new RoomImage
                {
                    RoomId = request.RoomId,
                    ImageUrl = url,
                    IsMain = false
                };

                _db.RoomImages.Add(image);

                result.Add(new Response
                {
                    ImageUrl = url,
                    IsMain = false
                });
            }

            await _db.SaveChangesAsync();

            return result;
        }
    }
}