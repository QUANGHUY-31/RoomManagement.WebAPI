using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Domain.Entities;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Rooms;

public class SetMainRoomImage
{
    public class Request
    {
        public int ImageId { get; set; }
    }

    public class Handler
    {
        private readonly AppDbContext _db;

        public Handler(AppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(Request request)
        {
            var image = await _db.RoomImages.FindAsync(request.ImageId);
            if (image == null)
                throw new Exception("Image not found");

            var images = await _db.RoomImages
                .Where(x => x.RoomId == image.RoomId)
                .ToListAsync();

            foreach (var img in images)
            {
                img.IsMain = false;
            }

            image.IsMain = true;

            await _db.SaveChangesAsync();
        }
    }
}