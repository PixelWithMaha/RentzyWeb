using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.DTOs;
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
        private readonly PaymentNotificationService _notificationService;

        public NotificationController(
       ITenantBookingService bookingService,
       TenantPaymentService paymentService,
       PaymentNotificationService notificationService) // ADD THIS
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
            _notificationService = notificationService; // ADD THIS
        }


        // GET: Direct payment from notification
        public async Task<IActionResult> Index()
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            try
            {
                // Use the notification service instead of payment service
                var notifications = await _notificationService.GetTenantNotificationsAsync(tenantId.Value);
                return View(notifications ?? new List<PaymentNotificationDTO>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Unable to load notifications at this time.";
                return View(new List<PaymentNotificationDTO>());
            }
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