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

        //public async Task<List<DateTime>> GetBookedDatesForPropertyAsync(int propertyId)
        //{
        //    // Get all bookings for this property that are active or confirmed
        //    var bookings = await _context.Bookings
        //        .Where(b => b.PropertyId == propertyId && b.StatusId == 1) // assuming 1 = Active
        //        .ToListAsync();

        //    var bookedDates = new List<DateTime>();

        //    foreach (var booking in bookings)
        //    {
        //        // Add each date between StartDate and EndDate to the list
        //        for (var date = booking.StartDate.Date; date <= booking.EndDate.Date; date = date.AddDays(1))
        //        {
        //            bookedDates.Add(date);
        //        }
        //    }

        //    return bookedDates.Distinct().ToList();
        //}
        //CHANGEEE
        //public async Task<List<DateTime>> GetBookedDatesForPropertyAsync(int propertyId)
        //{
        //    var bookedDates = new List<DateTime>();

        //    // Get dates from APPROVED rental requests (StatusId == 2 = Approved)
        //    var approvedRequests = await _context.PropertyRentalRequests
        //        .Where(r => r.PropertyId == propertyId && r.StatusId == 2) // Only this property, only approved
        //        .Select(r => new { r.StartDate, r.EndDate })
        //        .ToListAsync();

        //    foreach (var request in approvedRequests)
        //    {
        //        for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
        //        {
        //            bookedDates.Add(date);
        //        }
        //    }

        //    // Get dates from CONFIRMED bookings (StatusId == 1 = Active/Confirmed)
        //    var confirmedBookings = await _context.Bookings
        //        .Where(b => b.PropertyId == propertyId && b.StatusId == 1) // Only this property, only confirmed
        //        .Select(b => new { b.StartDate, b.EndDate })
        //        .ToListAsync();

        //    foreach (var booking in confirmedBookings)
        //    {
        //        for (var date = booking.StartDate.Date; date <= booking.EndDate.Date; date = date.AddDays(1))
        //        {
        //            bookedDates.Add(date);
        //        }
        //    }

        //    return bookedDates.Distinct().OrderBy(d => d).ToList();
        //}
        public async Task<List<DateTime>> GetBookedDatesForPropertyAsync(int propertyId)
        {
            var bookedDates = new List<DateTime>();

            Console.WriteLine($"=== CALCULATING BOOKED DATES FOR PROPERTY {propertyId} ===");

            // Get dates from APPROVED rental requests (StatusId == 2 = Approved)
            var approvedRequests = await _context.PropertyRentalRequests
                .Where(r => r.PropertyId == propertyId && r.StatusId == 2)
                .Select(r => new { r.StartDate, r.EndDate })
                .ToListAsync();

            Console.WriteLine($"Found {approvedRequests.Count} approved rental requests");

            foreach (var request in approvedRequests)
            {
                Console.WriteLine($"Processing approved request: {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}");

                // FIX: Use Date only (no time component) and include ALL dates in the range
                var currentDate = request.StartDate.Date;
                var endDate = request.EndDate.Date;

                while (currentDate <= endDate)
                {
                    bookedDates.Add(currentDate);
                    Console.WriteLine($"  Adding booked date: {currentDate:yyyy-MM-dd}");
                    currentDate = currentDate.AddDays(1);
                }
            }

            // Get dates from CONFIRMED bookings (StatusId == 1 = Active/Confirmed)
            var confirmedBookings = await _context.Bookings
                .Where(b => b.PropertyId == propertyId && b.StatusId == 1)
                .Select(b => new { b.StartDate, b.EndDate })
                .ToListAsync();

            Console.WriteLine($"Found {confirmedBookings.Count} confirmed bookings");

            foreach (var booking in confirmedBookings)
            {
                Console.WriteLine($"Processing confirmed booking: {booking.StartDate:yyyy-MM-dd} to {booking.EndDate:yyyy-MM-dd}");

                // FIX: Use Date only (no time component) and include ALL dates in the range
                var currentDate = booking.StartDate.Date;
                var endDate = booking.EndDate.Date;

                while (currentDate <= endDate)
                {
                    bookedDates.Add(currentDate);
                    Console.WriteLine($"  Adding booked date: {currentDate:yyyy-MM-dd}");
                    currentDate = currentDate.AddDays(1);
                }
            }

            var result = bookedDates.Distinct().OrderBy(d => d).ToList();
            Console.WriteLine($"Returning {result.Count} distinct booked dates for property {propertyId}");

            // Debug: Show final result
            foreach (var date in result)
            {
                Console.WriteLine($"  Final booked date: {date:yyyy-MM-dd}");
            }

            return result;
        }
    }
}
