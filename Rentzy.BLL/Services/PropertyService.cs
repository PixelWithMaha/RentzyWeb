using Microsoft.EntityFrameworkCore;
using Rentzy.BLL.DTOs;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository.Landlord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rentzy.DAL.Repository;

namespace Rentzy.BLL.Services
{
    public class PropertyService
    {
        private readonly ILandlordRepository _repo;


        public PropertyService(ILandlordRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
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
