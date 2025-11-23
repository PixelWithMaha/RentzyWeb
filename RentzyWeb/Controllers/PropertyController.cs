using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.Services;

namespace Rentzy.Web.Controllers
{
    public class PropertyController : Controller
    {
        private readonly PropertyService _propertyService;

        public PropertyController(PropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        //public IActionResult Index()
        //{
          //  var properties = _propertyService.GetAllProperties();
           // return View(properties);
        //}
    }
}
