using Rentzy.BLL.DTOs;
using Rentzy.BLL.DTOs.BookingDTOs;
using Rentzy.DAL.Models;
using Rentzy.DAL;
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
        public async Task<List<PropertyRentalRequestDto>> GetTenantRequestsAsync(int landlordId)
        {
            var requests = await _repo.GetTenantRequestsAsync(landlordId);

            return requests
                .Where(r => r.Status.Name == "Pending")  // <-- only pending
                .Select(r => new PropertyRentalRequestDto
                {
                    Id = r.Id,
                    TenantName = r.Tenant.FirstName + " " + r.Tenant.LastName,
                    PropertyTitle = r.Property.Title,
                    RequestedAt = r.RequestedAt,
                    Status = r.Status?.Name
                })
                .ToList();
        }

        public async Task<int> GetPendingRequestsCountAsync(int landlordId)
        {
            return await _repo.GetPendingTenantRequestsCountAsync(landlordId);
        }


        public async Task<bool> ApproveTenantRequestAsync(int requestId)
        {
            // 1️⃣ Get the request
            var request = await _repo.GetTenantRequestByIdAsync(requestId);
            if (request == null || request.Status.Name != "Pending")
                return false; // Already approved/rejected or not found

            // 2️⃣ Get approved status for PropertyRentalRequest
            var approvedRequestStatus = await _repo.GetRequestStatusByNameAsync("Approved");
            if (approvedRequestStatus == null) return false;

            request.StatusId = approvedRequestStatus.Id;

            // 3️⃣ Get "Active" booking status
            var activeBookingStatus = await _repo.GetBookingStatusByNameAsync("Active");
            if (activeBookingStatus == null) return false;

            // 4️⃣ Create a booking
            var booking = new Booking
            {
                TenantId = request.TenantId,
                PropertyId = request.PropertyId,
                StatusId = activeBookingStatus.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1)
            };

            // 5️⃣ Save changes
            await _repo.UpdateRequestAsync(request);
            await _repo.AddBookingAsync(booking);

            return true;
        }

        public async Task<Dictionary<string, List<TenantWithProperty>>> GetTenantsWithPropertyByStatusAsync(int landlordId)
        {
            return await _repo.GetTenantsWithPropertyByStatusAsync(landlordId);
        }




        public Task RejectTenantRequestAsync(int requestId) =>
            _repo.RejectTenantRequestAsync(requestId);

        // Dropdown helpers
        public Task<List<City>> GetAllCitiesAsync() => _propertyService.GetAllCitiesAsync();
        public Task<List<PropertyType>> GetAllPropertyTypesAsync() => _propertyService.GetAllPropertyTypesAsync();
    }
}
