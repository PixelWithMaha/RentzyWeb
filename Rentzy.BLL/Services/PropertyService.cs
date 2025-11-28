using Microsoft.EntityFrameworkCore;
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
        public async Task<IEnumerable<PropertyDTO>> GetAllPropertiesAsync()
        {
            var properties = await _propertyRepository.GetAllPropertiesAsync();
            return properties.Select(MapToDTO).ToList();
        }

        public async Task<IEnumerable<PropertyDTO>> SearchPropertiesByTypeAsync(string typeName)
        {
            var properties = await _propertyRepository.SearchByPropertyType(typeName);
            return properties.Select(MapToDTO).ToList();
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
            return properties.Select(p => new PropertyDTO
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Rent = p.MonthlyRent,
                CityId = p.CityId,
                PropertyTypeId = p.PropertyTypeId,
                LandlordId = p.LandlordId,
                Images = p.Images.ToList()
            }).ToList();
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


        // Dropdown helpers
        public Task<List<City>> GetAllCitiesAsync() => _repo.GetAllCitiesAsync();
        public Task<List<PropertyType>> GetAllPropertyTypesAsync() => _repo.GetAllPropertyTypesAsync();
    }
}
