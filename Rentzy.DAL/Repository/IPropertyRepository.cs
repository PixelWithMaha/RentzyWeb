// Repositories/IPropertyRepository.cs
using Rentzy.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository
{
    public interface IPropertyRepository
    {
        //Task<IEnumerable<Property>> GetAllPropertiesAsync();
        Task<List<Property>> GetAllPropertiesAsync();
        Task<IEnumerable<Property>> GetAllPropertiesByLandlordAsync(int landlordId);
        Task<IEnumerable<Property>>SearchByPropertyType(string typeName);
        Task<Property> GetPropertyByIdAsync(int id);
        Task AddPropertyAsync(Property property);
        Task UpdatePropertyAsync(Property property);
        Task DeletePropertyAsync(int id);

        Task AddPropertyImagesAsync(int propertyId, List<string> imageUrls);

        Task<IEnumerable<PropertyRentalRequest>> GetTenantRequestsAsync(int landlordId);
        Task UpdateRentalRequestStatusAsync(int requestId, string status);


        //Newww

        // New - rental/booking/payment related methods (match DAL models)
        Task<Property> GetPropertyDetailsAsync(int id);

        // rental request (PropertyRentalRequest)
        Task<int> AddRentalRequestAsync(PropertyRentalRequest request);
        Task<PropertyRentalRequest> GetRentalRequestAsync(int requestId);

        // booking creation
        Task<int> AddBookingAsync(Booking booking);

        // payment save
        Task AddPaymentAsync(Payment payment);

        // Get all booked dates for a property (from confirmed bookings)
        Task<List<DateTime>> GetBookedDatesForPropertyAsync(int propertyId);



    }
}
