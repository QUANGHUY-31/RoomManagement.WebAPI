
using RoomManagement.WebAPI.Common.Enums;
namespace RoomManagement.WebAPI.Domain.Entities
{
    public class Contract // quan trọng nhất hệ thống, vì nó liên kết giữa Room và Tenant, cũng như quản lý thời gian thuê và hóa đơn
    {
        public int Id { get; set; }

        // FK -> Room
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        // FK -> Tenant (người đứng tên)
        public int PrimaryTenantId { get; set; } // chỉ 1 người đứng tên hợp đồng
        public Tenant PrimaryTenant { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ContractStatus Status { get; set; } = ContractStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties

        // Danh sách người trong hợp đồng
        public List<ContractTenant> ContractTenants { get; set; } = new(); // tất cả người trong phòng

        // Danh sách hóa đơn
        public List<Bill> Bills { get; set; } = new();
    }
}
