using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.Services;
using Rentzy.DAL.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Rentzy.DAL.Repository.Approvals;

namespace RentzyWeb.Controllers
{
    public class BookingController : Controller
    {
        private readonly ITenantBookingService _bookingService;

        public BookingController(ITenantBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var model = await _bookingService.GetPropertyDetailsAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // REQUEST GET
        public async Task<IActionResult> Request(int propertyId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var bookedDates = await _bookingService.GetBookedDatesAsync(propertyId);
            var property = await _bookingService.GetPropertyDetailsAsync(propertyId);

            ViewBag.PropertyId = propertyId;
            ViewBag.PropertyTitle = property.Title;
            ViewBag.PropertyImage = property.Images?.FirstOrDefault()?.ImageUrl;
            ViewBag.BookedDates = bookedDates.Select(d => d.ToString("yyyy-MM-dd")).ToList();

            return View();
        }

        // REQUEST POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(int propertyId, DateTime startDate, DateTime endDate)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var request = new PropertyRentalRequest
            {
                PropertyId = propertyId,
                TenantId = tenantId.Value,
                StartDate = startDate,
                EndDate = endDate,
                StatusId = 2 // Pending approval
            };

            await _bookingService.CreateRentalRequestAsync(request);

            TempData["Success"] = "Rental request sent!";
            return RedirectToAction("Details", new { id = propertyId });
        }

        // PAYMENT GET
        public async Task<IActionResult> Payment(int requestId)
        {
            var model = await _bookingService.GetPaymentInfoAsync(requestId);
            if (model == null) return NotFound();
            return View(model);
        }

        // PAYMENT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int paymentId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var payment = await _bookingService.GetPaymentByIdAsync(paymentId);
            if (payment == null || payment.Booking.TenantId != tenantId.Value)
                return Forbid();

            await _bookingService.MarkPaymentAsPaidAsync(paymentId);

            return RedirectToAction("Receipt", new { requestId = payment.BookingId });
        }

        // RECEIPT
        public IActionResult Receipt(int requestId)
        {
            ViewBag.RequestId = requestId;
            return View();
        }
    }
}
