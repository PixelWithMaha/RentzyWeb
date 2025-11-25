using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentzy.BLL.DTOs;
using Rentzy.BLL.Services;
using Rentzy.DAL.Models;
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
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

            // Get properties
            var properties = await _propertyService.GetPropertiesByLandlordAsync(landlordId.Value);

            // Set ViewBag for cards
            ViewBag.TotalProperties = properties.Count;
            //ViewBag.ActiveTenants = await _landlordService.GetActiveTenantsCountAsync(landlordId.Value);
            //ViewBag.MonthlyRevenue = await _landlordService.GetMonthlyRevenueAsync(landlordId.Value);
            ViewBag.PendingRequests = await _landlordService.GetPendingRequestsCountAsync(landlordId.Value);

            // Pass properties to the view
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
        public async Task<IActionResult> UploadImages(int propertyId, List<IFormFile> imageFiles)
        {
            if (imageFiles != null && imageFiles.Any())
            {
                var imageUrls = new List<string>();
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/properties");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in imageFiles)
                {
                    var uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadPath, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    imageUrls.Add("/images/properties/" + uniqueFileName);
                }

                await _propertyService.UploadPropertyImagesAsync(propertyId, imageUrls);

                // Return JSON of uploaded images
                var result = imageUrls.Select(url => new { url }).ToList();
                return Json(result);
            }
            return BadRequest("No files uploaded");
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
            var success = await _landlordService.ApproveTenantRequestAsync(requestId);
            if (success)
                TempData["SuccessMessage"] = "Tenant request approved!";
            else
                TempData["ErrorMessage"] = "Approval failed.";

            return RedirectToAction("TenantRequests");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            await _landlordService.RejectTenantRequestAsync(requestId);
            return Ok(new { success = true });
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

        [HttpGet]
        public async Task<IActionResult> UpdateImages(int propertyId)
        {
            var property = await _propertyService.GetPropertyByIdAsync(propertyId);
            if (property == null) return NotFound();

            ViewBag.PropertyId = propertyId;
            return View(property); // Pass property with Images to the view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadNewImages(int propertyId, List<IFormFile> imageFiles)
        {
            if (imageFiles != null && imageFiles.Any())
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/properties");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                var uploadedImages = new List<PropertyImage>();
                foreach (var file in imageFiles)
                {
                    var uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadPath, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    uploadedImages.Add(new PropertyImage
                    {
                        PropertyId = propertyId,
                        ImageUrl = "/images/properties/" + uniqueFileName
                    });
                }

                // Save to DB
                await _propertyService.UploadPropertyImagesAsync(propertyId, uploadedImages.Select(i => i.ImageUrl).ToList());

                // Get IDs from DB (or include after save)
                var property = await _propertyService.GetPropertyByIdAsync(propertyId);
                var result = uploadedImages.Select(u => new { id = property.Images.LastOrDefault(i => i.ImageUrl == u.ImageUrl)?.Id, url = u.ImageUrl }).ToList();

                return Json(result);
            }
            return BadRequest("No files uploaded");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePropertyImage(int imageId)
        {
            await _propertyService.DeletePropertyImageAsync(imageId);
            return Ok(new { success = true }); // return JSON for JS to remove from UI
        }

        // Done button
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoneUpdatingImages(int propertyId)
        {
            return RedirectToAction("EditProperty", new { id = propertyId });
        }

    }
}
