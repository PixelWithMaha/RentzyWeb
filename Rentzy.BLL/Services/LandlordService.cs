using Rentzy.BLL.DTOs;
using Rentzy.BLL.DTOs.BookingDTOs;
using Rentzy.DAL;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository;
using Rentzy.DAL.Repository.Landlord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rentzy.DAL.Repository;
using Rentzy.DAL.Repositories;

namespace Rentzy.BLL.Services
{
    public class LandlordService
    {
        private readonly ILandlordRepository _repo;
        private readonly PropertyService _propertyService;
        private readonly IPropertyRepository _propertyRepo;
        private readonly PaymentService _paymentService;

        public LandlordService(ILandlordRepository repo, IPropertyRepository propertyRepo, PaymentService paymentService)
        {
            _repo = repo;
            _propertyRepo = propertyRepo;
            _propertyService = new PropertyService(repo, propertyRepo); // reuse core property logic
            _paymentService = paymentService;
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
                .Where(r => r.Status.Name == "Pending")
                .Select(r => new PropertyRentalRequestDto
                {
                    Id = r.Id,
                    TenantName = r.Tenant.FirstName + " " + r.Tenant.LastName,
                    PropertyTitle = r.Property.Title,
                    RequestedAt = r.RequestedAt,
                    StartDate = r.StartDate,   // added
                    EndDate = r.EndDate,       // added
                    Status = r.Status?.Name
                })
                .ToList();
        }


        public async Task<int> GetPendingRequestsCountAsync(int landlordId)
        {
            return await _repo.GetPendingTenantRequestsCountAsync(landlordId);
        }


        // Approve request, create booking & payment
        public async Task<bool> ApproveTenantRequestAsync(int requestId)
        {
            var request = await _repo.GetTenantRequestByIdAsync(requestId);
            if (request == null || request.Status.Name != "Pending") return false;

            var approvedStatus = await _repo.GetRequestStatusByNameAsync("Approved");
            if (approvedStatus == null) return false;

            request.StatusId = approvedStatus.Id;
            await _repo.UpdateRequestAsync(request);

            var activeBookingStatus = await _repo.GetBookingStatusByNameAsync("Active");
            if (activeBookingStatus == null) return false;

            var booking = new Booking
            {
                TenantId = request.TenantId,
                PropertyId = request.PropertyId,
                StatusId = activeBookingStatus.Id,
                StartDate = request.StartDate,  // use tenant dates
                EndDate = request.EndDate
            };

            await _repo.AddBookingAsync(booking);

            // Calculate number of months/days to compute initial payment
            var totalDays = (booking.EndDate - booking.StartDate).TotalDays;
            var totalMonths = Math.Ceiling(totalDays / 30); // approx monthly
            var amount = (decimal)totalMonths * request.Property.MonthlyRent;

            await _paymentService.CreateInitialPaymentAsync(booking.Id, amount);

            return true;
        }


        public async Task<Dictionary<string, List<TenantWithProperty>>> GetTenantsWithPropertyByStatusAsync(int landlordId)
        {
            return await _repo.GetTenantsWithPropertyByStatusAsync(landlordId);
        }

        public async Task<decimal> GetMonthlyRevenueAsync(int landlordId)
        {
            return await _repo.GetMonthlyRevenueAsync(landlordId);
        }



        public Task RejectTenantRequestAsync(int requestId) =>
            _repo.RejectTenantRequestAsync(requestId);

        // Dropdown helpers
        public Task<List<City>> GetAllCitiesAsync() => _propertyService.GetAllCitiesAsync();
        public Task<List<PropertyType>> GetAllPropertyTypesAsync() => _propertyService.GetAllPropertyTypesAsync();
    }
}
