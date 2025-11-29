using Rentzy.DAL.Models;

namespace Rentzy.DAL.Repository
{
    public interface IRentalRequestRepository
    {
        Task<IEnumerable<PropertyRentalRequest>> GetRequestsForLandlordAsync(int landlordId);
        Task<PropertyRentalRequest?> GetRequestByIdAsync(int requestId);
        Task UpdateRequestAsync(PropertyRentalRequest request);

        //NEWW
        Task AddRequestAsync(PropertyRentalRequest request);
        Task<IEnumerable<PropertyRentalRequest>> GetRequestsForTenantAsync(int tenantId);

        // return all booked dates (from confirmed Bookings) for calendar disabling
        Task<List<DateTime>> GetBookedDatesForPropertyAsync(int propertyId);

        // approved requests for tenant that await payment (notification list)
        Task<IEnumerable<PropertyRentalRequest>> GetApprovedRequestsAwaitingPaymentAsync(int tenantId);
    }
}
