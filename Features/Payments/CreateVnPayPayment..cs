using System;
using RoomManagement.WebAPI.Infrastructure;
using RoomManagement.WebAPI.Infrastructure.Payments.VnPay;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Domain.Entities;

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

        public Handler(AppDbContext db, IVnPayService vnPay)
        {
            _db = db;
            _vnPay = vnPay;
        }

        public string Handle(Request request, HttpContext context)
        {
            var bill = _db.Bills.Find(request.BillId);

            if (bill == null)
                throw new Exception("Bill không tồn tại");

            if (bill.Status == BillStatus.Paid)
                throw new Exception("Bill đã thanh toán");

            return _vnPay.CreatePaymentUrl(context, bill.Id, (long)bill.Total);
        }
    }
}
