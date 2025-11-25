using Rentzy.DAL.Models;

namespace Rentzy.DAL.Repository
{
    public interface IRentalRequestRepository
    {
        Task<IEnumerable<PropertyRentalRequest>> GetRequestsForLandlordAsync(int landlordId);
        Task<PropertyRentalRequest?> GetRequestByIdAsync(int requestId);
        Task UpdateRequestAsync(PropertyRentalRequest request);
    }
}
