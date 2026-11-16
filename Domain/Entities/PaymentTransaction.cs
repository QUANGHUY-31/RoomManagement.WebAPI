using RoomManagement.WebAPI.Common.Enums;

namespace RoomManagement.WebAPI.Domain.Entities;

public class PaymentTransaction
{
    public int Id { get; set; }

    public string TxnRef { get; set; } = string.Empty;

    public int BillId { get; set; }

    public Bill Bill { get; set; } = null!;

    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? VnPayTranNo { get; set; }

    public string? BankCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
}
