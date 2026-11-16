using RoomManagement.WebAPI.Common.Enums;
namespace RoomManagement.WebAPI.Domain.Entities
{
    public class Bill
    {
        public int Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        // FK -> Contract , bill gắn với hợp đồng chứ không gắn trực tiếp với phòng, hóa đơn phải gắn đúng hợp dồng tại thời điểm đó
        public int ContractId { get; set; }
        public Contract Contract { get; set; } = null!;

        public int ElectricityOld { get; set; }
        public int ElectricityNew { get; set; }
        public decimal Price { get; set; }

        public decimal ElectricityPrice { get; set; }

        public decimal WaterPrice { get; set; }

        public decimal ServicePrice { get; set; }

        public decimal Total { get; set; }

        public BillStatus Status { get; set; } = BillStatus.Unpaid;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // new 
        public DateTime? PaidAt { get; set; }

        public List<PaymentTransaction> PaymentTransactions { get; set; } = new();
    }
}
