namespace RoomManagement.WebAPI.Domain.Entities
{
    public class ContractTenant // trung gian giữa Contract và Tenant
    {
        public int Id { get; set; }

        // FK -> Contract , 1 hợp đồng có thể có nhiều người thuê, nhưng chỉ 1 người đứng tên hợp đồng (PrimaryTenantId trong Contract)
        public int ContractId { get; set; }
        public Contract Contract { get; set; } = null!;

        // FK -> Tenant , 1 người thuê có thể tham gia nhiều hợp đồng (ví dụ: chuyển phòng, hoặc ở nhiều phòng khác nhau theo thời gian)
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
    }
}
