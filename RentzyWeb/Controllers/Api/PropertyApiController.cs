using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.Services;
using System.Linq;
using System.Threading.Tasks;

namespace RentzyWeb.Controllers.Api
{
    [Route("api/property")]
    [ApiController]
    public class PropertyApiController : ControllerBase
    {
        private readonly RentalRequestService _rentalRequestService;

        public PropertyApiController(RentalRequestService rentalRequestService)
        {
            _rentalRequestService = rentalRequestService;
        }

        [HttpGet("{propertyId}/booked-dates")]
        public async Task<IActionResult> GetBookedDates(int propertyId)
        {
            var dates = await _rentalRequestService.GetBookedDatesAsync(propertyId);
            var strings = dates.Select(d => d.ToString("yyyy-MM-dd")).ToList();
            return Ok(strings);
        }
    }
}
