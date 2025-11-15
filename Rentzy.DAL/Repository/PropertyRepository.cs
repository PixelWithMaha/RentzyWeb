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

        public IEnumerable<Property> GetAllProperties()
        {
            return _context.Properties
                .Include(p => p.Landlord) // include landlord info
                .Include(p => p.TenantProperties)
                    .ThenInclude(tp => tp.Tenant) // include tenants
                .ToList();
        }

        public Property? GetPropertyById(int id)
        {
            return _context.Properties
                .Include(p => p.Landlord)
                .Include(p => p.TenantProperties)
                    .ThenInclude(tp => tp.Tenant)
                .FirstOrDefault(p => p.Id == id);
        }

        public void AddProperty(Property property)
        {
            _context.Properties.Add(property);
        }

        public void UpdateProperty(Property property)
        {
            _context.Properties.Update(property);
        }

        public void DeleteProperty(int id)
        {
            var property = _context.Properties.Find(id);
            if (property != null)
            {
                _context.Properties.Remove(property);
            }
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
