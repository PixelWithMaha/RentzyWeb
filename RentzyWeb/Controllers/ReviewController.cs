using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.DTOs;
using Rentzy.BLL.Exceptions;
using Rentzy.BLL.Services;
using Rentzy.Web.Authorization;
using System;
using System.Threading.Tasks;

namespace RentzyWeb.Controllers
{
    [AuthorizeRole("Tenant")]
    public class ReviewController : Controller
    {
        private readonly ReviewService _reviewService;

        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        public async Task<IActionResult> Submit(int propertyId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var model = await _reviewService.PrepareReviewFormAsync(propertyId, tenantId.Value);
                return View(model);
            }
            catch (ValidationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("MyBookings", "Tenant");
            }
            catch (Exception)
            {
                TempData["Error"] = "An unexpected error occurred loading the review form.";
                return RedirectToAction("MyBookings", "Tenant");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ReviewDTO model)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Force context consistency
            model.TenantId = tenantId.Value;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _reviewService.SubmitReviewAsync(model);
                TempData["Success"] = "Thank you! Your review has been submitted successfully.";
                return RedirectToAction("MyBookings", "Tenant");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An internal error occurred while saving your review. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var model = await _reviewService.GetReviewForEditAsync(id, tenantId.Value);
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "You are not authorized to edit this review.";
                return RedirectToAction("MyBookings", "Tenant");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("MyBookings", "Tenant");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ReviewDTO model)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            model.TenantId = tenantId.Value;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _reviewService.UpdateReviewAsync(model);
                TempData["Success"] = "Your review has been updated.";
                
                // Ideally return to property details, but since we only have propertyId we can redirect to its details page?
                // Wait, the property details page is at Booking/Details? Let me verify the route for property details.
                // If unknown, redirecting to MyBookings is safe as in Submit.
                return RedirectToAction("Details", "Booking", new { id = model.PropertyId });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "Unauthorized action attempt.";
                return RedirectToAction("MyBookings", "Tenant");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int propertyId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (!tenantId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                await _reviewService.DeleteReviewAsync(id, tenantId.Value);
                TempData["Success"] = "Review has been removed successfully.";
            }
            catch (UnauthorizedAccessException)
            {
                TempData["Error"] = "You do not have permission to delete this review.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error removing review: " + ex.Message;
            }

            return RedirectToAction("Details", "Booking", new { id = propertyId });
        }
    }
}
