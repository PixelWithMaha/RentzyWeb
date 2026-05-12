using Microsoft.EntityFrameworkCore;
using System;
using Rentzy.BLL.DTOs;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository;
using Rentzy.DAL.Repository.Approvals;
using Rentzy.DAL.Repository.Landlord;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services
{
    public class PropertyService
    {
        private readonly ILandlordRepository _repo;
        private readonly IPropertyRepository _propertyRepository;
        private readonly IPropertyApprovalRequestsRepo _RequestRepo;


        public PropertyService(ILandlordRepository repo,IPropertyRepository prepo, IPropertyApprovalRequestsRepo rep )
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _propertyRepository= prepo ?? throw new ArgumentNullException(nameof(prepo));
            _RequestRepo = rep;
        }
        //public async Task<IEnumerable<PropertyDTO>> GetAllPropertiesAsync()
        //{
        //    var properties = await _propertyRepository.GetAllPropertiesAsync();
        //    return properties.Select(MapToDTO).ToList();
        //}
        public async Task<IEnumerable<PropertyDTO>> GetAllPropertiesAsync()
        {
            var properties = await _propertyRepository.GetAllPropertiesAsync();

            var result = new List<PropertyDTO>();
            foreach (var p in properties)
            {
                // get latest approval request for this property (may be null)
                var approvalRequest = await _RequestRepo.GetByPropertyIdAsync(p.Id);

                var dto = new PropertyDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Rent = (int)p.MonthlyRent,
                    CityId = p.CityId,
                    PropertyTypeId = p.PropertyTypeId,
                    LandlordId = p.LandlordId,
                    LandlordName = p.Landlord != null ? p.Landlord.FirstName + " " + p.Landlord.LastName : "N/A",
                    TenantNames = p.Bookings?
                    .Where(b => b.StatusId == 1 && b.StartDate <= DateTime.Now && b.EndDate >= DateTime.Now)
                    .Select(b => b.Tenant.FirstName + " " + b.Tenant.LastName)
                    .ToList() ?? new List<string>(),
                    Images = p.Images?.ToList() ?? new List<PropertyImage>(),
                    StatusId = approvalRequest?.StatusId ?? ApprovalStatusConstants.Pending,
                    IsApproved = (approvalRequest?.StatusId == ApprovalStatusConstants.Approved)
                    
                };

                // Only include approved properties for tenant listings/search.
                if (dto.IsApproved)
                    result.Add(dto);
            }

            return result;
        }

        //public async Task<IEnumerable<PropertyDTO>> SearchPropertiesByTypeAsync(string typeName)
        //{
        //    var properties = await _propertyRepository.SearchByPropertyType(typeName);
        //    return properties.Select(MapToDTO).ToList();
        //}

        public async Task<IEnumerable<PropertyDTO>> SearchPropertiesByTypeAsync(string typeName)
        {
            var properties = await _propertyRepository.SearchByPropertyType(typeName);

            var result = new List<PropertyDTO>();
            foreach (var p in properties)
            {
                var approvalRequest = await _RequestRepo.GetByPropertyIdAsync(p.Id);

                var dto = new PropertyDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Rent = (int)p.MonthlyRent,
                    CityId = p.CityId,
                    PropertyTypeId = p.PropertyTypeId,
                    LandlordId = p.LandlordId,
                    LandlordName = p.Landlord != null ? p.Landlord.FirstName + " " + p.Landlord.LastName : "N/A",
                    TenantNames = p.Bookings?
                    .Where(b => b.StatusId == 1 && b.StartDate <= DateTime.Now && b.EndDate >= DateTime.Now)
                    .Select(b => b.Tenant.FirstName + " " + b.Tenant.LastName)
                    .ToList() ?? new List<string>(),
                    Images = p.Images?.ToList() ?? new List<PropertyImage>(),
                    StatusId = approvalRequest?.StatusId ?? ApprovalStatusConstants.Pending,
                    IsApproved = (approvalRequest?.StatusId == ApprovalStatusConstants.Approved)
                };

                if (dto.IsApproved)
                    result.Add(dto);
            }

            return result;
        }

        private PropertyDTO MapToDTO(Rentzy.DAL.Models.Property p)
        {
            if (p == null) return null;

            return new PropertyDTO
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Rent = (int)p.MonthlyRent,
                CityId = p.CityId,
                PropertyTypeId = p.PropertyTypeId,
                LandlordId = p.LandlordId,
                LandlordName = p.Landlord != null ? p.Landlord.FirstName + " " + p.Landlord.LastName : "N/A",
                TenantNames = p.Bookings?.Select(b => b.Tenant.FirstName + " " + b.Tenant.LastName).ToList() ?? new List<string>(),
                Images = p.Images?.ToList() ?? new List<PropertyImage>()
            };
        }



        // CRUD
        public async Task<List<PropertyDTO>> GetPropertiesByLandlordAsync(int landlordId)
        {
            var properties = await _repo.GetPropertiesByLandlordAsync(landlordId);

            var result = new List<PropertyDTO>();

            foreach (var p in properties)
            {
                // Fetch latest approval request for this property
                var approvalRequest = await _RequestRepo.GetByPropertyIdAsync(p.Id);

                result.Add(new PropertyDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Rent = p.MonthlyRent,
                    CityId = p.CityId,
                    CityName=p.City.Name,
                    PropertyTypeName=p.PropertyType.Name,
                    PropertyTypeId = p.PropertyTypeId,
                    LandlordId = p.LandlordId,
                    Images = p.Images.ToList(),
                    StatusId = approvalRequest?.StatusId ?? ApprovalStatusConstants.Pending
                });
            }

            return result;
        }


        public async Task<PropertyDTO> GetPropertyByIdAsync(int propertyId)
        {
            var p = await _repo.GetPropertyByIdAsync(propertyId);
            if (p == null) return null;
            return new PropertyDTO
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Rent = p.MonthlyRent,
                CityId = p.CityId,
                PropertyTypeId = p.PropertyTypeId,
                LandlordId = p.LandlordId,
                Images = p.Images.ToList()
            };
        }

        public async Task AddPropertyAsync(PropertyDTO dto)
        {
            var property = new Property
            {
                Title = dto.Title,
                Description = dto.Description,
                MonthlyRent = dto.Rent,
                CityId = dto.CityId,
                PropertyTypeId = dto.PropertyTypeId,
                LandlordId = dto.LandlordId
            };

            await _repo.AddPropertyAsync(property);

            var request = new PropertyApprovalRequest
            {
                Comments = "Admin has not viewed yet.",
                PropertyId = property.Id,
                StatusId = ApprovalStatusConstants.Pending,
                RequestedAt = DateTime.UtcNow,
            };

            await _RequestRepo.CreateAsync(request);
            await _RequestRepo.SaveChangesAsync();
        }

        public async Task UpdatePropertyAsync(PropertyDTO dto)
        {
            var property = new Property
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                MonthlyRent = dto.Rent,
                CityId = dto.CityId,
                PropertyTypeId = dto.PropertyTypeId,
                LandlordId = dto.LandlordId
            };
            await _repo.UpdatePropertyAsync(property);
        }

        public async Task DeletePropertyAsync(int propertyId)
        {
            await _repo.DeletePropertyAsync(propertyId);
        }

        public async Task UploadPropertyImagesAsync(int propertyId, List<string> imageUrls)
        {
            await _repo.UploadPropertyImagesAsync(propertyId, imageUrls);
        }

        public Task DeletePropertyImageAsync(int imageId)
        {
            return _repo.DeletePropertyImageAsync(imageId);
        }
        //NEWWW

        public async Task<List<DateTime>> GetBookedDatesAsync(int propertyId)
        {
            return await _propertyRepository.GetBookedDatesForPropertyAsync(propertyId);
        }

        //public async Task<PropertyDTO> GetPropertyDetailsAsync(int propertyId)
        //{
        //    var p = await _propertyRepository.GetPropertyDetailsAsync(propertyId);
        //    if (p == null) return null;

        //    return new PropertyDTO
        //    {
        //        Id = p.Id,
        //        Title = p.Title,
        //        Description = p.Description,
        //        // p.MonthlyRent is your model field — cast/convert to int if DTO expects int
        //        Rent = (int)p.MonthlyRent,
        //        CityId = p.CityId,
        //        PropertyTypeId = p.PropertyTypeId,
        //        LandlordId = p.LandlordId,
        //        LandlordName = p.Landlord != null ? p.Landlord.FirstName + " " + p.Landlord.LastName : "N/A",
        //        TenantNames = p.Bookings?.Select(b => b.Tenant.FirstName + " " + b.Tenant.LastName).ToList() ?? new List<string>(),
        //        Images = p.Images?.ToList() ?? new List<PropertyImage>()
        //    };
        //}
        public async Task<PropertyDTO> GetPropertyDetailsAsync(int propertyId)
        {
            var p = await _propertyRepository.GetPropertyDetailsAsync(propertyId);
            if (p == null) return null;

            var approvalRequest = await _RequestRepo.GetByPropertyIdAsync(p.Id);

            return new PropertyDTO
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Rent = (int)p.MonthlyRent,
                CityId = p.CityId,
                PropertyTypeId = p.PropertyTypeId,
                LandlordId = p.LandlordId,
                LandlordName = p.Landlord != null ? p.Landlord.FirstName + " " + p.Landlord.LastName : "N/A",
                TenantNames = p.Bookings?
                .Where(b => b.StatusId == 1 && b.StartDate <= DateTime.Now && b.EndDate >= DateTime.Now)
                .Select(b => b.Tenant.FirstName + " " + b.Tenant.LastName)
                .ToList() ?? new List<string>(),
                Images = p.Images?.ToList() ?? new List<PropertyImage>(),
                StatusId = approvalRequest?.StatusId ?? ApprovalStatusConstants.Pending,
                IsApproved = (approvalRequest?.StatusId == ApprovalStatusConstants.Approved)
            };
        }


        public async Task<int> CreateRentalRequestAsync(int tenantId, int propertyId)
        {
            var req = new PropertyRentalRequest
            {
                TenantId = tenantId,
                PropertyId = propertyId,
                StatusId = 1, // set to Pending (ensure ApprovalStatus with id 1 exists)
                RequestedAt = DateTime.Now
            };

            return await _propertyRepository.AddRentalRequestAsync(req);
        }


        public async Task<PaymentDTO> GetPaymentInfoAsync(int requestId)
        {
            var req = await _propertyRepository.GetRentalRequestAsync(requestId);
            if (req == null) return null;

            return new PaymentDTO
            {
                RequestId = req.Id,
                PropertyId = req.PropertyId,
                PropertyTitle = req.Property?.Title ?? string.Empty,
                TenantId = req.TenantId,
                TenantName = req.Tenant != null ? req.Tenant.FirstName + " " + req.Tenant.LastName : string.Empty,
                Amount = req.Property != null ? (decimal)req.Property.MonthlyRent : 0m,
                DefaultPaymentMethodId = 1,
                RequestedAt = req.RequestedAt
            };
        }


        public async Task ProcessPaymentAsync(int requestId, decimal amount)
        {
            var req = await _propertyRepository.GetRentalRequestAsync(requestId);
            if (req == null) throw new InvalidOperationException("Rental request not found.");

            // Create booking (you may adjust StartDate/EndDate logic to your requirements)
            var booking = new Booking
            {
                TenantId = req.TenantId,
                PropertyId = req.PropertyId,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                StatusId = 1 // Active (ensure BookingStatus with id 1 exists)
            };

            int bookingId = await _propertyRepository.AddBookingAsync(booking);

            // Create payment
            var payment = new Payment
            {
                BookingId = bookingId,
                Amount = amount,
                PaidAt = DateTime.Now,
                PaymentMethodId = 1, // default
                StatusId = 1 // Paid (or Pending depending on your logic)
            };

            await _propertyRepository.AddPaymentAsync(payment);
        }



        // Dropdown helpers
        public Task<List<City>> GetAllCitiesAsync() => _repo.GetAllCitiesAsync();
        public Task<List<PropertyType>> GetAllPropertyTypesAsync() => _repo.GetAllPropertyTypesAsync();
    }
}
