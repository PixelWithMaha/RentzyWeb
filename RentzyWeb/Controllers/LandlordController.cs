using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.DTOs;
using Rentzy.BLL.Services;
using Rentzy.Web.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rentzy.Web.Controllers
{
    [AuthorizeRole("Landlord")]
    public class LandlordController : Controller
    {
        private readonly PropertyService _propertyService;
        private readonly LandlordService _landlordService;

        public LandlordController(PropertyService propertyService, LandlordService landlordService)
        {
            _propertyService = propertyService;
            _landlordService = landlordService;
        }

        // LANDLORD DASHBOARD
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var landlordId = HttpContext.Session.GetInt32("UserId");
            if (landlordId == null) return RedirectToAction("Login", "Account");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // load properties list for dashboard
            var properties = await _propertyService.GetPropertiesByLandlordAsync(landlordId.Value);
            ViewBag.Properties = properties;

            return View();
        }

        // ADD PROPERTY PAGE
        [HttpGet]
        public async Task<IActionResult> AddProperty()
        {
            ViewBag.Cities = await _propertyService.GetAllCitiesAsync();
            ViewBag.PropertyTypes = await _propertyService.GetAllPropertyTypesAsync();
            return View(new PropertyDTO());
        }

        // ADD PROPERTY ACTION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProperty(PropertyDTO dto, List<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cities = await _propertyService.GetAllCitiesAsync();
                ViewBag.PropertyTypes = await _propertyService.GetAllPropertyTypesAsync();
                return View(dto);
            }

            var landlordId = HttpContext.Session.GetInt32("UserId");
            if (landlordId == null)
                return RedirectToAction("Login", "Account");

            dto.LandlordId = landlordId.Value;

            await _propertyService.AddPropertyAsync(dto);

            var addedProperty = (await _propertyService.GetPropertiesByLandlordAsync(landlordId.Value))
                                .FirstOrDefault(p => p.Title == dto.Title && p.Description == dto.Description);

            if (addedProperty != null && images?.Count > 0)
            {
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/properties");

                // Ensure directory exists
                if (!Directory.Exists(imageFolder))
                    Directory.CreateDirectory(imageFolder);

                var imageUrls = new List<string>();
                foreach (var file in images)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); // unique name
                    var filePath = Path.Combine(imageFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    imageUrls.Add("/images/properties/" + fileName);
                }

                await _propertyService.UploadPropertyImagesAsync(addedProperty.Id, imageUrls);
            }

            TempData["Success"] = "Property added successfully!";
            return RedirectToAction("MyProperties");
        }

        // EDIT PROPERTY
        [HttpGet]
        public async Task<IActionResult> EditProperty(int id)
        {
            var property = await _propertyService.GetPropertyByIdAsync(id);
            if (property == null) return NotFound();

            ViewBag.Cities = await _propertyService.GetAllCitiesAsync();
            ViewBag.PropertyTypes = await _propertyService.GetAllPropertyTypesAsync();

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(PropertyDTO dto)
        {
            var landlordId = HttpContext.Session.GetInt32("UserId");
            if (landlordId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            dto.LandlordId = landlordId.Value; // ensure valid FK

            await _propertyService.UpdatePropertyAsync(dto);
            TempData["SuccessMessage"] = "Property updated successfully!";
            return RedirectToAction("Dashboard");
        }


        // DELETE PROPERTY
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            await _propertyService.DeletePropertyAsync(id);
            TempData["SuccessMessage"] = "Property deleted successfully!";
            return RedirectToAction("Dashboard");
        }

        // UPLOAD IMAGES
        [HttpGet]
        public IActionResult UploadImages(int propertyId)
        {
            ViewBag.PropertyId = propertyId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImages(int propertyId, List<string> imageUrls)
        {
            await _propertyService.UploadPropertyImagesAsync(propertyId, imageUrls);
            TempData["SuccessMessage"] = "Images uploaded successfully!";
            return RedirectToAction("Dashboard");
        }

        // TENANT REQUESTS
        [HttpGet]
        public async Task<IActionResult> TenantRequests()
        {
            var landlordId = HttpContext.Session.GetInt32("UserId");
            if (landlordId == null) return RedirectToAction("Login", "Account");

            var requests = await _landlordService.GetTenantRequestsAsync(landlordId.Value);
            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(int requestId)
        {
            await _landlordService.ApproveTenantRequestAsync(requestId);
            TempData["SuccessMessage"] = "Tenant request approved!";
            return RedirectToAction("TenantRequests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            await _landlordService.RejectTenantRequestAsync(requestId);
            TempData["ErrorMessage"] = "Tenant request rejected!";
            return RedirectToAction("TenantRequests");
        }

        // MY PROPERTIES PAGE
        [HttpGet]
        public async Task<IActionResult> MyProperties()
        {
            var landlordId = HttpContext.Session.GetInt32("UserId");
            if (landlordId == null)
                return RedirectToAction("Login", "Account");

            var properties = await _propertyService.GetPropertiesByLandlordAsync(landlordId.Value);
            return View(properties);
        }
    }
}
