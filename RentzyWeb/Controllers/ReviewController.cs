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
    }
}
