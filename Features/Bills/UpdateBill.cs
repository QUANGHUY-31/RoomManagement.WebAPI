using Microsoft.EntityFrameworkCore;
using RoomManagement.WebAPI.Common.Enums;
using RoomManagement.WebAPI.Infrastructure;

namespace RoomManagement.WebAPI.Features.Bills;

public class UpdateBill
{
    public class Request
    {
        public int ElectricityOld { get; set; }
        public int ElectricityNew { get; set; }
        public decimal ElectricityPrice { get; set; }
        public decimal WaterPrice { get; set; }
        public decimal ServicePrice { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class Response
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int ElectricityOld { get; set; }
        public int ElectricityNew { get; set; }
        public decimal Price { get; set; }
        public decimal ElectricityPrice { get; set; }
        public decimal WaterPrice { get; set; }
        public decimal ServicePrice { get; set; }
        public decimal Total { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class Handler
    {
        private readonly AppDbContext _context;

        public Handler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(int id, Request request)
        {
            if (request.ElectricityOld < 0 || request.ElectricityNew < request.ElectricityOld)
                throw new Exception("Chỉ số điện không hợp lệ");

            if (request.ElectricityPrice < 0 || request.WaterPrice < 0 || request.ServicePrice < 0)
                throw new Exception("Các khoản phí không được nhỏ hơn 0");

            if (request.Month < 1 || request.Month > 12 || request.Year < 1)
                throw new Exception("Tháng hoặc năm không hợp lệ");

            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null)
                throw new Exception("Hóa đơn không tồn tại");

            if (bill.Status != BillStatus.Unpaid)
                throw new Exception("Chỉ được sửa hóa đơn chưa thanh toán");

            var duplicatePeriod = await _context.Bills
                .AnyAsync(b =>
                    b.Id != id &&
                    b.ContractId == bill.ContractId &&
                    b.Month == request.Month &&
                    b.Year == request.Year);

            if (duplicatePeriod)
                throw new Exception("Hóa đơn tháng này đã tồn tại");

            bill.ElectricityOld = request.ElectricityOld;
            bill.ElectricityNew = request.ElectricityNew;
            bill.ElectricityPrice = request.ElectricityPrice;
            bill.WaterPrice = request.WaterPrice;
            bill.ServicePrice = request.ServicePrice;
            bill.Month = request.Month;
            bill.Year = request.Year;
            bill.Total =
                ((request.ElectricityNew - request.ElectricityOld) * request.ElectricityPrice) +
                request.WaterPrice +
                request.ServicePrice +
                bill.Price;

            await _context.SaveChangesAsync();

            return new Response
            {
                Id = bill.Id,
                ContractId = bill.ContractId,
                ElectricityOld = bill.ElectricityOld,
                ElectricityNew = bill.ElectricityNew,
                Price = bill.Price,
                ElectricityPrice = bill.ElectricityPrice,
                WaterPrice = bill.WaterPrice,
                ServicePrice = bill.ServicePrice,
                Total = bill.Total,
                Month = bill.Month,
                Year = bill.Year,
                Status = bill.Status.ToString()
            };
        }
    }
}
