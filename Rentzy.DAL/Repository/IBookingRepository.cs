using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rentzy.DAL.Repository
{
    public interface IBookingRepository
    {
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking> GetBookingByIdAsync(int id);
        Task AddBookingAsync(Booking booking);
        Task UpdateBookingAsync(Booking booking);
        Task DeleteBookingAsync(int id);

        // Optional: additional methods
        Task<List<Booking>> GetBookingsByTenantIdAsync(int tenantId);
        Task<List<Booking>> GetBookingsByPropertyIdAsync(int propertyId);
    }
}
