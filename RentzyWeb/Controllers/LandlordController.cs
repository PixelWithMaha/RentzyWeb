using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentzy.BLL.DTOs;
using Rentzy.BLL.Services;
using Rentzy.DAL.Models;
using Rentzy.BLL.Services.ReportsServices;
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
        private readonly PaymentService _paymentService;
        private readonly ReportsService _reportsService;
        private readonly AuthService _authService;

        public LandlordController(
            AuthService authService,
            PropertyService propertyService,
            LandlordService landlordService,
            PaymentService payservice,
            ReportsService reportsService)
        {
            _propertyService = propertyService;
            _landlordService = landlordService;
            _paymentService = payservice;
            _reportsService = reportsService;
            _authService = authService;
        }

        // LANDLORD DASHBOARD
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var landlordId = HttpContext.Session.GetInt32("UserId");
            if (landlordId == null) return RedirectToAction("Login", "Account");

            try
            {
                // Check if landlord is verified
                var isVerified = await _authService.IsLandlordVerifiedAsync(landlordId.Value);
                if (!isVerified)
                {
                    return RedirectToAction("PendingApproval");
                }

                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");

                // Get all properties for stats/cards
                var allProperties = await _propertyService.GetPropertiesByLandlordAsync(landlordId.Value);
                ViewBag.TotalProperties = allProperties.Count;

                // Only show approved properties in Dashboard section
                var approvedProperties = allProperties.Where(p => p.StatusId == ApprovalStatusConstants.Approved).ToList();
                ViewBag.Properties = approvedProperties;



                ViewBag.MonthlyRevenue = await _landlordService.GetMonthlyRevenueAsync(landlordId.Value);
                ViewBag.PendingRequests = await _landlordService.GetPendingRequestsCountAsync(landlordId.Value);

                var activeTenants = await _landlordService.GetTenantsWithPropertyByStatusAsync(landlordId.Value);
                ViewBag.ActiveTenants = activeTenants.ContainsKey("Active") ? activeTenants["Active"].Count : 0;

                // Reports (Charts)
                var data = await _reportsService.GetDashboardReportsForLandlordAsync(landlordId.Value);

                // Booking Status Pie
                ViewBag.BookingStatusLabels = data.BookingStatusCount.Select(x => x.Status).ToList();
                ViewBag.BookingStatusData = data.BookingStatusCount.Select(x => x.Count).ToList();

                // Monthly Bookings Bar
                ViewBag.MonthlyBookingLabels = data.MonthlyBookings.Select(x => $"{x.Month}/{x.Year}").ToList();
                ViewBag.MonthlyBookingData = data.MonthlyBookings.Select(x => x.Count).ToList();

                // Monthly Revenue Line
                ViewBag.MonthlyRevenueLabels = data.MonthlyRevenue.Select(x => $"{x.Month}/{x.Year}").ToList();
                ViewBag.MonthlyRevenueData = data.MonthlyRevenue.Select(x => x.TotalRevenue).ToList();

                return View();
            }
            catch
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        public IActionResult PendingApproval()
        {
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
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

            // Save property + trigger approval request
            await _propertyService.AddPropertyAsync(dto);

            var addedProperty = (await _propertyService.GetPropertiesByLandlordAsync(landlordId.Value))
                                .FirstOrDefault(p => p.Title == dto.Title && p.Description == dto.Description);

            if (addedProperty != null && images?.Count > 0)
            {
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/properties");
                if (!Directory.Exists(imageFolder))
                    Directory.CreateDirectory(imageFolder);

                var imageUrls = new List<string>();
                foreach (var file in images)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(imageFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    imageUrls.Add("/images/properties/" + fileName);
                }

                await _propertyService.UploadPropertyImagesAsync(addedProperty.Id, imageUrls);
            }

            // ✅ Only set success message after saving property
            TempData["SuccessMessage"] = "Property details have been sent to the admin for approval!";

            // Redirect to MyProperties page to show the message
            return RedirectToAction("MyProperties");
        }


        // ------- REMAINING METHODS UNCHANGED ---------

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
            if (landlordId == null) return RedirectToAction("Login", "Account");

            dto.LandlordId = landlordId.Value;

            await _propertyService.UpdatePropertyAsync(dto);
            TempData["SuccessMessage"] = "Property updated successfully!";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProperty(int id)
        {
            await _propertyService.DeletePropertyAsync(id);
            TempData["SuccessMessage"] = "Property deleted successfully!";
            return RedirectToAction("MyProperties");
        }

        // IMAGE UPLOADS
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

                var result = imageUrls.Select(url => new { url }).ToList();
                return Json(result);
            }
            return BadRequest("No files uploaded");
        }

        // TENANTS
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

        [HttpGet]
        public async Task<IActionResult> Tenants()
        {
            var landlordId = HttpContext.Session.GetInt32("UserId");
            if (landlordId == null) return RedirectToAction("Login", "Account");

            var tenantsByStatus = await _landlordService.GetTenantsWithPropertyByStatusAsync(landlordId.Value);
            return View(tenantsByStatus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int requestId)
        {
            await _landlordService.RejectTenantRequestAsync(requestId);
            return Ok(new { success = true });
        }

        // MY PROPERTIES
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
            return View(property);
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

                await _propertyService.UploadPropertyImagesAsync(propertyId, uploadedImages.Select(i => i.ImageUrl).ToList());
                var property = await _propertyService.GetPropertyByIdAsync(propertyId);

                var result = uploadedImages.Select(u => new
                {
                    id = property.Images.LastOrDefault(i => i.ImageUrl == u.ImageUrl)?.Id,
                    url = u.ImageUrl
                }).ToList();

                return Json(result);
            }
            return BadRequest("No files uploaded");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePropertyImage(int imageId)
        {
            await _propertyService.DeletePropertyImageAsync(imageId);
            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoneUpdatingImages(int propertyId)
        {
            return RedirectToAction("EditProperty", new { id = propertyId });
        }
    }
}
