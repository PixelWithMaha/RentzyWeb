using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;

namespace Rentzy.DAL.Repository
{
    public class RentalRequestRepository : IRentalRequestRepository
    {
        private readonly RentzyDBContext _db;

        public RentalRequestRepository(RentzyDBContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<PropertyRentalRequest>> GetRequestsForLandlordAsync(int landlordId)
        {
            return await _db.PropertyRentalRequests
                .Include(r => r.Tenant)
                .Include(r => r.Property)
                .Include(r => r.Status)
                .Where(r => r.Property.LandlordId == landlordId)
                .ToListAsync();
        }

        public async Task<PropertyRentalRequest?> GetRequestByIdAsync(int requestId)
        {
            return await _db.PropertyRentalRequests
                .Include(r => r.Tenant)
                .Include(r => r.Property)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        public async Task UpdateRequestAsync(PropertyRentalRequest request)
        {
            _db.PropertyRentalRequests.Update(request);
            await _db.SaveChangesAsync();
        }
    }
}
