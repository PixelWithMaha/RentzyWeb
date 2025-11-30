using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RentzyWeb.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ITenantBookingService _bookingService;
        private readonly TenantPaymentService _paymentService;

        public NotificationController(ITenantBookingService bookingService, TenantPaymentService paymentService)
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
        }

        // GET: View all notifications with pending payments
        // GET: View all notifications with pending payments
        public async Task<IActionResult> Index()
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            try
            {
                var pendingPayments = await _paymentService.GetPendingPaymentsAsync(tenantId.Value);

                // FIX: Remove the problematic ?? operator and handle null properly
                if (pendingPayments == null)
                {
                    return View(new List<Rentzy.BLL.DTOs.PaymentDTO>());
                }

                // Filter only pending payments (StatusId = 1)
                var actualPendingPayments = pendingPayments.Where(p => p.StatusId == 1).ToList();

                return View(actualPendingPayments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unable to load notifications at this time.";
                return View(new List<Rentzy.BLL.DTOs.PaymentDTO>());
            }
        }

        // GET: Direct payment from notification
        public async Task<IActionResult> PayFromNotification(int paymentId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var payment = await _bookingService.GetPaymentByIdAsync(paymentId);
            if (payment == null) return NotFound();

            // FIX: Get the booking to check tenant authorization
            var booking = await _bookingService.GetBookingByPaymentIdAsync(paymentId);
            if (booking == null || booking.TenantId != tenantId.Value)
                return Forbid();

            // Check payment status
            if (payment.StatusId == 2) // Paid
            {
                TempData["Info"] = "This payment has already been processed.";
                return RedirectToAction("Receipt", "Booking", new { paymentId = paymentId });
            }

            if (payment.StatusId == 3) // Failed
            {
                TempData["Info"] = "Previous payment failed. You can try again.";
            }

            return RedirectToAction("ConfirmPayment", "Booking", new { paymentId = paymentId });
        }

        // GET: View payment details from notification
        public async Task<IActionResult> PaymentDetails(int paymentId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var payment = await _bookingService.GetPaymentByIdAsync(paymentId);
            if (payment == null) return NotFound();

            // FIX: Get the booking to check tenant authorization
            var booking = await _bookingService.GetBookingByPaymentIdAsync(paymentId);
            if (booking == null || booking.TenantId != tenantId.Value)
                return Forbid();

            return View(payment);
        }

        // GET: Check for new payment notifications (AJAX)
        public async Task<IActionResult> CheckNotifications()
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return Json(new { authenticated = false });

            try
            {
                var pendingPayments = await _paymentService.GetPendingPaymentsAsync(tenantId.Value);

                // Count only pending payments (StatusId = 1)
                var pendingCount = pendingPayments?.Count(p => p.StatusId == 1) ?? 0;

                return Json(new
                {
                    authenticated = true,
                    count = pendingCount,
                    hasNotifications = pendingCount > 0
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    authenticated = true,
                    count = 0,
                    hasNotifications = false
                });
            }
        }
    }
}