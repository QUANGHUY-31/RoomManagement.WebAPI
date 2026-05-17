namespace RoomManagement.WebAPI.Domain.Entities
{
    public class Tenant
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string CCCD { get; set; } = string.Empty; // unique

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Liên kết với User
        public int? UserId { get; set; }

        public User? User { get; set; }

        // Thuộc tính điều hướng
        public List<ContractTenant> ContractTenants { get; set; } = new();

        public List<Contract> PrimaryContracts { get; set; } = new();
    }
}
