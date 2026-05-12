using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository
{
    public class BookingRepository : IBookingRepository
    {
        private readonly RentzyDBContext _context;

        public BookingRepository(RentzyDBContext context)
        {
            _context = context;
        }

        public async Task<List<Booking>> GetAllBookingsAsync() =>
            await _context.Bookings.ToListAsync();

        public async Task<Booking> GetBookingByIdAsync(int id) =>
            await _context.Bookings.FindAsync(id);

        public async Task AddBookingAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBookingAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBookingAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Booking>> GetBookingsByTenantIdAsync(int tenantId) =>
            await _context.Bookings
                          .Where(b => b.TenantId == tenantId)
                          .ToListAsync();

        public async Task<List<Booking>> GetBookingsByPropertyIdAsync(int propertyId) =>
            await _context.Bookings
                          .Where(b => b.PropertyId == propertyId)
                          .ToListAsync();
    }
}
