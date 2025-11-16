using Rentzy.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Rentzy.DAL.Context;

namespace Rentzy.DAL.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly RentzyDBContext _context;

        public PropertyRepository(RentzyDBContext context)
        {
            _context = context;
        }

        // Get all properties with related data
        public IEnumerable<Property> GetAllProperties()
        {
            return _context.Properties
                .Include(p => p.Landlord) // Landlord info
                .Include(p => p.Images)    // Property images
                .Include(p => p.Bookings)
                    .ThenInclude(b => b.Tenant) // Booked tenants
                .Include(p => p.RentalRequests)
                    .ThenInclude(r => r.Tenant) // Rental request tenants
                .Include(p => p.ApprovalRequests)
                    .ThenInclude(a => a.Admin)  // Admin who approved/rejected
                .ToList();
        }

        // Get a property by ID with related data
        public Property? GetPropertyById(int id)
        {
            return _context.Properties
                .Include(p => p.Landlord)
                .Include(p => p.Images)
                .Include(p => p.Bookings)
                    .ThenInclude(b => b.Tenant)
                .Include(p => p.RentalRequests)
                    .ThenInclude(r => r.Tenant)
                .Include(p => p.ApprovalRequests)
                    .ThenInclude(a => a.Admin)
                .FirstOrDefault(p => p.Id == id);
        }

        // Add a new property
        public void AddProperty(Property property)
        {
            _context.Properties.Add(property);
        }

        // Update an existing property
        public void UpdateProperty(Property property)
        {
            _context.Properties.Update(property);
        }

        // Delete a property by ID
        public void DeleteProperty(int id)
        {
            var property = _context.Properties.Find(id);
            if (property != null)
            {
                _context.Properties.Remove(property);
            }
        }

        // Save changes to the database
        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
