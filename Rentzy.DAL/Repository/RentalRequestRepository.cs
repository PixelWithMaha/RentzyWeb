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
            .ThenInclude(p => p.Images)        
        .Include(r => r.Property)
            .ThenInclude(p => p.City)         
        .Include(r => r.Property)
            .ThenInclude(p => p.PropertyType)  
        .Include(r => r.Property)
            .ThenInclude(p => p.Landlord)    
        .Include(r => r.Status)
        .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        public async Task UpdateRequestAsync(PropertyRentalRequest request)
        {
            _db.PropertyRentalRequests.Update(request);
            await _db.SaveChangesAsync();
        }

        //NEWW
        public async Task AddRequestAsync(PropertyRentalRequest request)
        {
            _db.PropertyRentalRequests.Add(request);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<PropertyRentalRequest>> GetRequestsForTenantAsync(int tenantId)
        {
            return await _db.PropertyRentalRequests
                .Include(r => r.Property)
                .Include(r => r.Status)
                .Where(r => r.TenantId == tenantId)
                .ToListAsync();
        }

        public async Task<List<DateTime>> GetBookedDatesForPropertyAsync(int propertyId)
        {
            // expand all confirmed bookings into dates set
            var bookings = await _db.Bookings
                .Where(b => b.PropertyId == propertyId)
                .Select(b => new { b.StartDate, b.EndDate })
                .ToListAsync();

            var dates = new HashSet<DateTime>();
            foreach (var b in bookings)
            {
                var start = b.StartDate.Date;
                var end = b.EndDate.Date;
                for (var d = start; d <= end; d = d.AddDays(1))
                    dates.Add(d);
            }
            return dates.OrderBy(d => d).ToList();
        }

        public async Task<IEnumerable<PropertyRentalRequest>> GetApprovedRequestsAwaitingPaymentAsync(int tenantId)
        {
            // adjust this condition to match your ApprovalStatus semantics
            var approvedStatus = await _db.ApprovalStatuses
                .FirstOrDefaultAsync(s => s.Name.ToLower() == "approved");

            if (approvedStatus == null) return Enumerable.Empty<PropertyRentalRequest>();

            // If booking is created only after payment, then approved requests are pending payment.
            // If a booking is created by landlord on approval, you'll need to filter out ones already booked.
            var list = await _db.PropertyRentalRequests
                .Include(r => r.Property)
                .Include(r => r.Status)
                .Where(r => r.TenantId == tenantId && r.StatusId == approvedStatus.Id)
                .ToListAsync();

            return list;
        }

    }
}
