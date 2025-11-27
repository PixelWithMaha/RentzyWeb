using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Rentzy.Web.Controllers
{
    public class PropertyController : Controller
    {
        private readonly PropertyService _propertyService;

        public PropertyController(PropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        public async Task<IActionResult> Index(string searchType)
        {
            IEnumerable<Rentzy.BLL.DTOs.PropertyDTO> props;

            if (!string.IsNullOrWhiteSpace(searchType))
                props = await _propertyService.SearchPropertiesByTypeAsync(searchType);
            else
                props = await _propertyService.GetAllPropertiesAsync();

            return View(props);
        }
    }
}
