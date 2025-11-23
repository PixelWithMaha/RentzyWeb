// Repositories/PropertyRepository.cs
using Microsoft.EntityFrameworkCore;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly RentzyDBContext _context;

        public PropertyRepository(RentzyDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Property>> GetAllPropertiesByLandlordAsync(int landlordId)
        {
            return await _context.Properties
                        .Include(p => p.Images)
                        .Where(p => p.LandlordId == landlordId)
                        .ToListAsync();
        }

        public async Task<Property> GetPropertyByIdAsync(int id)
        {
            return await _context.Properties
                        .Include(p => p.Images)
                        .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddPropertyAsync(Property property)
        {
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePropertyAsync(Property property)
        {
            _context.Properties.Update(property);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePropertyAsync(int id)
        {
            var prop = await _context.Properties.FindAsync(id);
            if (prop != null)
            {
                _context.Properties.Remove(prop);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddPropertyImagesAsync(int propertyId, List<string> imageUrls)
        {
            var images = imageUrls.Select(url => new PropertyImage
            {
                PropertyId = propertyId,
                ImageUrl = url
            }).ToList();

            _context.PropertyImages.AddRange(images);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PropertyRentalRequest>> GetTenantRequestsAsync(int landlordId)
        {
            return await _context.PropertyRentalRequests
                .Include(r => r.Property)
                .Where(r => r.Property.LandlordId == landlordId)
                .ToListAsync();
        }

        public async Task UpdateRentalRequestStatusAsync(int requestId, string status)
        {
            var request = await _context.PropertyRentalRequests
                                        .Include(r => r.Status)
                                        .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request != null)
            {
                var approvalStatus = await _context.ApprovalStatuses
                                                   .FirstOrDefaultAsync(a => a.Name == status);
                if (approvalStatus != null)
                {
                    request.Status = approvalStatus;
                    await _context.SaveChangesAsync();
                }
            }
        }

    }
}
