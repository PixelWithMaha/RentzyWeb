using Rentzy.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository
{
    public interface IReviewRepository
    {
        Task AddReviewAsync(Review review);
        Task<bool> HasCompletedBookingAsync(int tenantId, int propertyId);
        Task<IEnumerable<Review>> GetReviewsByPropertyIdAsync(int propertyId);
        Task<Dictionary<int, (double AverageRating, int ReviewCount)>> GetReviewAggregatesAsync(IEnumerable<int> propertyIds);
    }
}
