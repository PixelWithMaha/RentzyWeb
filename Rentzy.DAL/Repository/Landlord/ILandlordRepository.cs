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
        Task ApproveTenantRequestAsync(int requestId);
        Task RejectTenantRequestAsync(int requestId);

        // Dropdown helpers
        Task<List<City>> GetAllCitiesAsync();
        Task<List<PropertyType>> GetAllPropertyTypesAsync();
    }
}
