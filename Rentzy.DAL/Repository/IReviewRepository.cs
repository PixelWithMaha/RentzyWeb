using Rentzy.DAL.Models;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository
{
    public interface IReviewRepository
    {
        Task AddReviewAsync(Review review);
        Task<bool> HasCompletedBookingAsync(int tenantId, int propertyId);
    }
}
