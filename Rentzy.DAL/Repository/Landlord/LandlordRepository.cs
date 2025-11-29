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

        public Task<List<Property>> GetPropertiesByLandlordAsync(int landlordId)
        {
            return _context.Properties
                .Include(p => p.City)
                .Include(p => p.PropertyType)
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
                .Include(r => r.Property)
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

        public async Task<Dictionary<string, List<TenantWithProperty>>> GetTenantsWithPropertyByStatusAsync(int landlordId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Tenant)
                .Include(b => b.Status)
                .Include(b => b.Property)
                .Where(b => b.Property.LandlordId == landlordId)
                .ToListAsync();

            var grouped = bookings
                .GroupBy(b => b.Status.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(b => new TenantWithProperty
                    {
                        Tenant = b.Tenant,
                        Property = b.Property,
                        Booking = b               // 🔥 FIX 1 — Map the booking!
                    })
                    .ToList()
                );

            return grouped;
        }

        // LandlordRepository.cs
        public async Task<decimal> GetMonthlyRevenueAsync(int landlordId)
        {
            var activeBookings = await _context.Bookings
                .Include(b => b.Property)
                .Include(b => b.Status)
                .Where(b => b.Property.LandlordId == landlordId && b.Status.Name == "Active")
                .ToListAsync();

            return activeBookings.Sum(b => (decimal?)b.Property.MonthlyRent) ?? 0m;
        }


    }



}
