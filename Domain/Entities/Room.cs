using System.Diagnostics.Contracts;
using RoomManagement.WebAPI.Common.Enums;

namespace RoomManagement.WebAPI.Domain.Entities
{
    public class Room
    {
        public int Id { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public RoomStatus Status { get; set; } = RoomStatus.Empty;

        public int MaxOccupancy { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Thuộc tính điều hướng    
        public List<Contract> Contracts { get; set; } = new();

        public List<RoomImage> Images { get; set; } = new();
    }
}
