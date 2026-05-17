using RoomManagement.WebAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace RoomManagement.WebAPI.Features.Contracts
{
    public class ContractDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int PrimaryTenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class GetContracts
    {
        public class Handler
        {
            private readonly AppDbContext _db;

            public Handler(AppDbContext db)
            {
                _db = db;
            }

            public async Task<List<ContractDto>> Handle()
            {
                return await _db.Contracts
                    .Select(c => new ContractDto
                    {
                        Id = c.Id,
                        RoomId = c.RoomId,
                        PrimaryTenantId = c.PrimaryTenantId,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate
                    })
                    .ToListAsync();
            }
        }
    }
}
