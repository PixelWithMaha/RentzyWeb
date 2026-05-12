using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly RentzyDBContext _context;

        public ReviewRepository(RentzyDBContext context)
        {
            _context = context;
        }

        public async Task AddReviewAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasCompletedBookingAsync(int tenantId, int propertyId)
        {
            // In this application, a completed booking correlates directly to a PropertyRentalRequest
            // record marked with StatusId = 4 (Completed).
            return await _context.PropertyRentalRequests
                .AnyAsync(r => r.TenantId == tenantId 
                            && r.PropertyId == propertyId 
                            && r.StatusId == 4); // 4 explicitly mapped to Completed
        }
    }
}
