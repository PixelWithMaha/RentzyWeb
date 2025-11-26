// Services/IPropertyService.cs
using Rentzy.BLL.DTOs;
using Rentzy.BLL.DTOs.BookingDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services
{
    public interface IPropertyService
    {
        Task<IEnumerable<PropertyDTO>> GetPropertiesByLandlordAsync(int landlordId);
        Task<IEnumerable<PropertyDTO>> GetAllPropertiesAsync();
        Task<IEnumerable<PropertyDTO>> SearchPropertiesByTypeAsync(string typeName);
        Task<PropertyDTO> GetPropertyByIdAsync(int id);
        Task AddPropertyAsync(PropertyDTO propertyDto);
        Task UpdatePropertyAsync(PropertyDTO propertyDto);
        Task DeletePropertyAsync(int id);
        Task UploadPropertyImagesAsync(int propertyId, List<string> imageUrls);
        Task<IEnumerable<PropertyRentalRequestDto>> GetTenantRequestsAsync(int landlordId);
        Task UpdateRentalRequestStatusAsync(int requestId, string status);
    }
}
