using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Domain.Entities;

namespace RoomManagement.WebAPI.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ContractTenant> ContractTenants { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }

        //SQL Server không cho phép tồn tại nhiều đường dẫn xóa dây chuyền (cascade paths), vì vậy tôi đã cấu hình DeleteBehavior.Restrict để kiểm soát việc xóa dữ liệu một cách rõ ràng.
        // Khi xóa một Tenant, tôi sẽ phải đảm bảo rằng tất cả các ContractTenant liên quan đến Tenant đó được xử lý trước (ví dụ: xóa chúng hoặc cập nhật để không còn tham chiếu đến Tenant đó) trước khi xóa Tenant.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ContractTenant>()
                .HasOne(ct => ct.Tenant)
                .WithMany(t => t.ContractTenants)
                .HasForeignKey(ct => ct.TenantId)
                .OnDelete(DeleteBehavior.Restrict); // 🔥 FIX Ở ĐÂY

            modelBuilder.Entity<Contract>()
               .HasOne(c => c.PrimaryTenant)
               .WithMany(t => t.PrimaryContracts)
               .HasForeignKey(c => c.PrimaryTenantId)
               .OnDelete(DeleteBehavior.Restrict);
            // thêm quan hệ một-một giữa Tenant và User
            modelBuilder.Entity<Tenant>()
               .HasOne(t => t.User)
               .WithOne(u => u.Tenant)
               .HasForeignKey<Tenant>(t => t.UserId)
               .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PaymentTransaction>()
                .HasOne(payment => payment.Bill)
                .WithMany(bill => bill.PaymentTransactions)
                .HasForeignKey(payment => payment.BillId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentTransaction>()
                .HasIndex(payment => payment.TxnRef)
                .IsUnique();

            modelBuilder.Entity<PaymentTransaction>()
                .Property(payment => payment.TxnRef)
                .HasMaxLength(100);

            modelBuilder.Entity<PaymentTransaction>()
                .Property(payment => payment.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaymentTransaction>()
                .Property(payment => payment.VnPayTranNo)
                .HasMaxLength(100);

            modelBuilder.Entity<PaymentTransaction>()
                .Property(payment => payment.BankCode)
                .HasMaxLength(50);
        }
    }
}