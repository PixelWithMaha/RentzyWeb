using Rentzy.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository.Landlord
{
    public interface ILandlordRepository
    {
        Task<List<Property>> GetPropertiesByLandlordAsync(int landlordId);
        Task<Property> GetPropertyByIdAsync(int propertyId);
        Task AddPropertyAsync(Property property);
        Task UpdatePropertyAsync(Property property);
        Task DeletePropertyAsync(int propertyId);
        Task UploadPropertyImagesAsync(int propertyId, List<string> imageUrls);
        Task<List<PropertyRentalRequest>> GetTenantRequestsAsync(int landlordId);
        Task<int> GetPendingTenantRequestsCountAsync(int landlordId);
        Task RejectTenantRequestAsync(int requestId);
        Task DeletePropertyImageAsync(int imageId);
        Task<PropertyRentalRequest?> GetTenantRequestByIdAsync(int requestId);
        Task UpdateRequestAsync(PropertyRentalRequest request);
        Task AddBookingAsync(Booking booking);
        Task<ApprovalStatus?> GetStatusByNameAsync(string statusName);
        Task<bool> ApproveTenantRequestAsync(int requestId);
        Task<ApprovalStatus?> GetRequestStatusByNameAsync(string name);
        Task<BookingStatus?> GetBookingStatusByNameAsync(string name);
        Task<Dictionary<string, List<TenantWithProperty>>> GetTenantsWithPropertyByStatusAsync(int landlordId);


        // Dropdown helpers
        Task<List<City>> GetAllCitiesAsync();
        Task<List<PropertyType>> GetAllPropertyTypesAsync();
    }
}
