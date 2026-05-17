namespace RoomManagement.WebAPI.Domain.Entities
{
    public class RoomImage
    {
        public int Id { get; set; }

        // FK -> Room , quan hệ 1 phòng có thể có nhiều ảnh, nhưng 1 ảnh chỉ thuộc về 1 phòng
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty; // Lưu link ảnh (Cloudinary, Firebase…)

        public bool IsMain { get; set; } = false; // Ảnh đại diện của phòng
    }
}
