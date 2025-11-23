// Repositories/IPropertyRepository.cs
using Rentzy.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository
{
    public interface IPropertyRepository
    {
        Task<IEnumerable<Property>> GetAllPropertiesByLandlordAsync(int landlordId);
        Task<Property> GetPropertyByIdAsync(int id);
        Task AddPropertyAsync(Property property);
        Task UpdatePropertyAsync(Property property);
        Task DeletePropertyAsync(int id);

        Task AddPropertyImagesAsync(int propertyId, List<string> imageUrls);

        Task<IEnumerable<PropertyRentalRequest>> GetTenantRequestsAsync(int landlordId);
        Task UpdateRentalRequestStatusAsync(int requestId, string status);
    }
}
