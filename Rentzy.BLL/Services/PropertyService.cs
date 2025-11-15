using Rentzy.DAL.Models;
using Rentzy.DAL.Repositories;
using System.Collections.Generic;



namespace Rentzy.BLL.Services
{
    public class PropertyService
    {
        private readonly IPropertyRepository _propertyRepository;

        public PropertyService(IPropertyRepository propertyRepository)
        {
            _propertyRepository = propertyRepository;
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return _propertyRepository.GetAllProperties();
        }
    }
}
