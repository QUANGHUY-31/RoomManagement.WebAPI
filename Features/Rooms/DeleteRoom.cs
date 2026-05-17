using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Infrastructure;
using RoomManagement.WebAPI.Common.Enums;

namespace RoomManagement.WebAPI.Features.Rooms;

public class DeleteRoom
{
    public class Request
    {
        public int Id { get; set; }
    }

    public class Handler
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(Request request)
        {
            // 🔥 1. Tìm phòng
            var room = await _context.Rooms
                .Include(r => r.Contracts)
                .FirstOrDefaultAsync(r => r.Id == request.Id);

            if (room == null)
                throw new Exception("Phòng không tồn tại");

            // 🔥 2. Kiểm tra trạng thái nếu có hợp đồng đang hoạt động không thể xóa
            var hasActiveContract = room.Contracts
                .Any(c => c.Status == ContractStatus.Active);

            if (hasActiveContract)
                throw new Exception("Không thể xóa phòng đang có người thuê");

            // 🔥 3. Xóa
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }
}
