using System.Data;
using System.Globalization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Domain.Entities;
using RoomManagement.WebAPI.Infrastructure;
using RoomManagement.WebAPI.Infrastructure.Payments.VnPay;

namespace RoomManagement.WebAPI.Features.Payments;

public class CreateVnPayPayment
{
    public class Request
    {
        public int BillId { get; set; }
    }

    public class Handler
    {
        private readonly AppDbContext _db;
        private readonly IVnPayService _vnPay;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Handler(
            AppDbContext db,
            IVnPayService vnPay,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _vnPay = vnPay;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> Handle(Request request)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;
            var userIdValue = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
                throw new UnauthorizedAccessException("Không xác định được tài khoản đăng nhập");

            await using var dbTransaction =
                await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var bill = await _db.Bills
                .Include(b => b.Contract)
                    .ThenInclude(c => c.ContractTenants)
                        .ThenInclude(ct => ct.Tenant)
                .FirstOrDefaultAsync(b => b.Id == request.BillId);

            if (bill == null)
                throw new InvalidOperationException("Hóa đơn không tồn tại");

            if (bill.Status != BillStatus.Unpaid)
                throw new InvalidOperationException("Chỉ có thể thanh toán hóa đơn chưa thanh toán");

            var isAdmin = user!.IsInRole(UserRole.Admin.ToString());
            if (!isAdmin &&
                (bill.Contract.Status != ContractStatus.Active ||
                 !bill.Contract.ContractTenants.Any(ct => ct.Tenant.UserId == userId)))
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thanh toán hóa đơn này");
            }

            if (bill.Total <= 0 || bill.Total != decimal.Truncate(bill.Total))
                throw new InvalidOperationException("Tổng tiền hóa đơn phải là số nguyên dương theo đơn vị VND");

            var amount = checked((long)bill.Total);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var txnRef = $"{bill.Id}_{timestamp}";
            while (await _db.PaymentTransactions.AnyAsync(payment => payment.TxnRef == txnRef))
            {
                timestamp = Math.Max(
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    timestamp + 1);
                txnRef = $"{bill.Id}_{timestamp}";
            }

            var payment = new PaymentTransaction
            {
                TxnRef = txnRef,
                BillId = bill.Id,
                Amount = bill.Total,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.PaymentTransactions.Add(payment);
            await _db.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return _vnPay.CreatePaymentUrl(
                httpContext!,
                txnRef,
                amount);
        }
    }
}
