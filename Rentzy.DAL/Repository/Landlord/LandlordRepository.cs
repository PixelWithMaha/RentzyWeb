using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rentzy.DAL.Repository.Landlord;

namespace Rentzy.DAL.Repository
{
    public class LandlordRepository : ILandlordRepository
    {
        private readonly RentzyDBContext _context;

        public LandlordRepository(RentzyDBContext context)
        {
            _context = context;
        }

        public async Task<List<Property>> GetPropertiesByLandlordAsync(int landlordId)
        {
            return await _context.Properties
                .Include(p => p.Images)
                .Where(p => p.LandlordId == landlordId)
                .ToListAsync();
        }

        public async Task<Property> GetPropertyByIdAsync(int propertyId)
        {
            return await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == propertyId);
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

        public async Task DeletePropertyAsync(int propertyId)
        {
            var property = await _context.Properties.FindAsync(propertyId);
            if (property != null)
            {
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UploadPropertyImagesAsync(int propertyId, List<string> imageUrls)
        {
            foreach (var url in imageUrls)
            {
                _context.PropertyImages.Add(new PropertyImage
                {
                    PropertyId = propertyId,
                    ImageUrl = url
                });
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<PropertyRentalRequest>> GetTenantRequestsAsync(int landlordId)
        {
            return await _context.PropertyRentalRequests
                .Include(r => r.Property)
                .Include(r => r.Tenant)
                .Include(r => r.Status)
                .Where(r => r.Property.LandlordId == landlordId)
                .ToListAsync();
        }

        public async Task<PropertyRentalRequest?> GetTenantRequestByIdAsync(int requestId)
        {
            return await _context.PropertyRentalRequests
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        // DAL / Repository
        public async Task<bool> ApproveTenantRequestAsync(int requestId)
        {
            var request = await _context.PropertyRentalRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return false;

            var approvedStatus = await _context.ApprovalStatuses
                .FirstOrDefaultAsync(s => s.Name == "Approved");

            if (approvedStatus == null) return false;

            if (request.StatusId == approvedStatus.Id)
                return false; // already approved

            request.StatusId = approvedStatus.Id;  // critical line

            var changes = await _context.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<int> GetPendingTenantRequestsCountAsync(int landlordId)
        {
            return await _context.PropertyRentalRequests
                .Include(r => r.Property)
                .Where(r => r.Property.LandlordId == landlordId && r.Status.Name == "Pending")
                .CountAsync();
        }



        public async Task RejectTenantRequestAsync(int requestId)
        {
            var request = await _context.PropertyRentalRequests
                .Include(r => r.Status)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request != null)
            {
                var rejectedStatus = await _context.ApprovalStatuses
                    .FirstOrDefaultAsync(s => s.Name == "Rejected");

                request.Status = rejectedStatus;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePropertyImageAsync(int imageId)
        {
            var image = await _context.PropertyImages
                                      .SingleOrDefaultAsync(i => i.Id == imageId);
            if (image != null)
            {
                // Remove from DB
                _context.PropertyImages.Remove(image);
                await _context.SaveChangesAsync();

                // Remove file from wwwroot
                var filePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    image.ImageUrl.TrimStart('/').Replace("/", "\\")
                );
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        public async Task<ApprovalStatus?> GetStatusByNameAsync(string statusName)
        {
            return await _context.ApprovalStatuses.FirstOrDefaultAsync(s => s.Name == statusName);
        }

        public async Task AddBookingAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRequestAsync(PropertyRentalRequest request)
        {
            _context.PropertyRentalRequests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task<ApprovalStatus?> GetRequestStatusByNameAsync(string name)
        {
            return await _context.ApprovalStatuses.FirstOrDefaultAsync(s => s.Name == name);
        }

        public async Task<BookingStatus?> GetBookingStatusByNameAsync(string name)
        {
            return await _context.BookingStatuses.FirstOrDefaultAsync(s => s.Name == name);
        }
        public async Task<List<City>> GetAllCitiesAsync()
        {
            return await _context.Cities.ToListAsync();
        }

        public async Task<List<PropertyType>> GetAllPropertyTypesAsync()
        {
            return await _context.PropertyTypes.ToListAsync();
        }
    }
}
