using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<Review>> GetReviewsByPropertyIdAsync(int propertyId)
        {
            return await _context.Reviews
                .Include(r => r.Tenant)
                .Where(r => r.PropertyId == propertyId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<int, (double AverageRating, int ReviewCount)>> GetReviewAggregatesAsync(IEnumerable<int> propertyIds)
        {
            // Convert IEnumerable to array/list to ensure EF translates containment correctly.
            var idList = propertyIds?.ToList() ?? new List<int>();
            
            if (!idList.Any()) return new Dictionary<int, (double, int)>();

            var stats = await _context.Reviews
                .Where(r => idList.Contains(r.PropertyId))
                .GroupBy(r => r.PropertyId)
                .Select(g => new 
                {
                    PropertyId = g.Key,
                    Count = g.Count(),
                    // Explicit cast to double to avoid integer division issues
                    Average = g.Average(r => (double)r.Rating) 
                })
                .ToListAsync();

            return stats.ToDictionary(
                x => x.PropertyId, 
                x => (x.Average, x.Count)
            );
        }
    }
}
