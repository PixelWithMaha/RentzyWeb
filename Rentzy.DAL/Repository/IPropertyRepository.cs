using Rentzy.DAL.Models;
using System.Collections.Generic;

namespace Rentzy.DAL.Repositories
{
    public interface IPropertyRepository
    {
        IEnumerable<Property> GetAllProperties();
        Property? GetPropertyById(int id);
        void AddProperty(Property property);
        void UpdateProperty(Property property);
        void DeleteProperty(int id);
        void SaveChanges();
    }
}
