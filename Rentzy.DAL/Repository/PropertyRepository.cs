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

        public async Task<IEnumerable<Property>> SearchByPropertyType(string typeName)
        {
            return  _context.Properties
                .Include(p => p.Landlord)
                .Include(p => p.PropertyType)
                .Include(p => p.Bookings)
                    .ThenInclude(b => b.Tenant)
                .Where(p => p.PropertyType.Name.Contains(typeName))
                .ToList();
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
               .Include(r => r.Tenant)
               .Include(r => r.Status)
               .Where(r => r.Property.LandlordId == landlordId
                        && r.Status.Name == "Pending")  // only pending requests
               .OrderByDescending(r => r.RequestedAt)
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

        //public async Task<IEnumerable<Property>> GetAllPropertiesAsync()
        //{
        //    return await _context.Properties
        //        .Include(p => p.Images)
        //        .Include(p => p.PropertyType)
        //        .Include(p => p.Landlord)
        //        .ToListAsync();
        //}

        public async Task<List<Property>> GetAllPropertiesAsync()
        {
            return await _context.Properties
                .Include(p => p.Landlord)                  // Include landlord details
                .Include(p => p.Bookings)                  // Include bookings
                .ThenInclude(b => b.Tenant)           // Include tenant details in bookings
                .Include(p => p.Images)                    // Include property images
                .Include(p => p.PropertyType)              // Include property type
                .ToListAsync();
        }

        //Newww
        // returns property with related data
        public async Task<Property> GetPropertyDetailsAsync(int id)
        {
            return await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Landlord)
                .Include(p => p.Bookings)
                    .ThenInclude(b => b.Tenant)
                .Include(p => p.PropertyType)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // add a rental request (PropertyRentalRequest)
        public async Task<int> AddRentalRequestAsync(PropertyRentalRequest request)
        {
            _context.PropertyRentalRequests.Add(request);
            await _context.SaveChangesAsync();
            return request.Id;
        }

        // get a rental request including property and tenant
        public async Task<PropertyRentalRequest> GetRentalRequestAsync(int requestId)
        {
            return await _context.PropertyRentalRequests
                .Include(r => r.Property)
                .Include(r => r.Tenant)
                .FirstOrDefaultAsync(r => r.Id == requestId);
        }

        // create booking (returns booking id)
        public async Task<int> AddBookingAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking.Id;
        }

        // save payment
        public async Task AddPaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
        }

    }
}
