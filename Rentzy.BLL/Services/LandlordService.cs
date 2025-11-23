using Rentzy.BLL.DTOs;
using Rentzy.BLL.DTOs.BookingDTOs;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository.Landlord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services
{
    public class LandlordService
    {
        private readonly ILandlordRepository _repo;
        private readonly PropertyService _propertyService;

        public LandlordService(ILandlordRepository repo)
        {
            _repo = repo;
            _propertyService = new PropertyService(repo); // reuse core property logic
        }

        // Property CRUD delegated to PropertyService
        public Task<List<PropertyDTO>> GetPropertiesByLandlordAsync(int landlordId) =>
            _propertyService.GetPropertiesByLandlordAsync(landlordId);

        public Task<PropertyDTO> GetPropertyByIdAsync(int propertyId) =>
            _propertyService.GetPropertyByIdAsync(propertyId);

        public Task AddPropertyAsync(PropertyDTO dto) =>
            _propertyService.AddPropertyAsync(dto);

        public Task UpdatePropertyAsync(PropertyDTO dto) =>
            _propertyService.UpdatePropertyAsync(dto);

        public Task DeletePropertyAsync(int propertyId) =>
            _propertyService.DeletePropertyAsync(propertyId);

        public Task UploadPropertyImagesAsync(int propertyId, List<string> imageUrls) =>
            _propertyService.UploadPropertyImagesAsync(propertyId, imageUrls);

        // Tenant Requests
        public async Task<List<RentalRequestDTO>> GetTenantRequestsAsync(int landlordId)
        {
            var requests = await _repo.GetTenantRequestsAsync(landlordId);
            return requests.Select(r => new RentalRequestDTO
            {
                Id = r.Id,
                PropertyId = r.PropertyId,
                PropertyTitle = r.Property.Title,
                TenantId = r.TenantId,
                Status = r.Status?.Name
            }).ToList();
        }

        public Task ApproveTenantRequestAsync(int requestId) =>
            _repo.ApproveTenantRequestAsync(requestId);

        public Task RejectTenantRequestAsync(int requestId) =>
            _repo.RejectTenantRequestAsync(requestId);

        // Dropdown helpers
        public Task<List<City>> GetAllCitiesAsync() => _propertyService.GetAllCitiesAsync();
        public Task<List<PropertyType>> GetAllPropertyTypesAsync() => _propertyService.GetAllPropertyTypesAsync();
    }
}
