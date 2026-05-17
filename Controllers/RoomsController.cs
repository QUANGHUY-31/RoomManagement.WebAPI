using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomManagement.WebAPI.Features.Rooms;

namespace RoomManagement.WebAPI.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    //[Authorize] // 🔐 tất cả API cần login
    public class RoomsController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Get(
        [FromServices] GetRooms.Handler handler,
        [FromQuery] string? status = null)
        {
            var result = await handler.Handle(status);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(
        [FromBody] CreateRoom.Request request,
        [FromServices] CreateRoom.Handler handler)
        {
            var result = await handler.Handle(request);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
                public async Task<IActionResult> Delete(
                int id,
                [FromServices] DeleteRoom.Handler handler)
         {
            await handler.Handle(new DeleteRoom.Request { Id = id });
            return Ok("Xóa thành công");
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(
        int id,
        [FromServices] GetRoomDetail.Handler handler)
        {
            var result = await handler.Handle(id);
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("images/{roomId}")]
        public async Task<IActionResult> UploadImages(
        int roomId,
        [FromForm] List<IFormFile> files,
        [FromServices] UploadRoomImages.Handler handler)
        {
            var result = await handler.Handle(new UploadRoomImages.Request
            {
                RoomId = roomId,
                Files = files
            });

            return Ok(result);
        }
        // lấy ảnh chính của phòng
        [Authorize(Roles = "Admin")]
        [HttpPut("images/{imageId}/set-main")]
        public async Task<IActionResult> SetMain(int imageId,
            [FromServices] SetMainRoomImage.Handler handler)
        {
            await handler.Handle(new SetMainRoomImage.Request
            {
                ImageId = imageId
            });

            return Ok();
        }
        /// API trả về cho người dùng
        [Authorize]
        [HttpGet("my-room")]
        public async Task<IActionResult> GetMyRoom(
        [FromServices] MyRoom.Handler handler)
        {
            var result = await handler.Handle();

            return Ok(result);
        }
    }
}
