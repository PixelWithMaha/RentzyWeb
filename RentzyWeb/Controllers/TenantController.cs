using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.Services;
using Rentzy.Web.Authorization;
using Rentzy.DAL.Models;
using Rentzy.DAL.Repository;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using RentzyWeb.ViewModels;
using System.Collections.Generic;

namespace RentzyWeb.Controllers
{
    [AuthorizeRole("Tenant")]
    public class TenantController : Controller
    {
        private readonly PaymentNotificationService _notifService;
        private readonly TenantPaymentService _paymentService;
        private readonly IRentalRequestRepository _rentalRequestRepo;
        private readonly IPropertyRepository _propertyRepo;
        private readonly ReviewService _reviewService;

        public TenantController(
            PaymentNotificationService notifService,
            TenantPaymentService paymentService,
            IRentalRequestRepository rentalRequestRepo,
            IPropertyRepository propertyRepo,
            ReviewService reviewService)
        {
            _notifService = notifService;
            _paymentService = paymentService;
            _rentalRequestRepo = rentalRequestRepo;
            _propertyRepo = propertyRepo;
            _reviewService = reviewService;
        }

        // GET: Tenant/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var rentedPropertiesCount = await _paymentService.GetRentedPropertiesCountAsync(tenantId.Value);
            var activeContractsCount = await _paymentService.GetActiveContractsCountAsync(tenantId.Value);

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.MyPropertiesCount = rentedPropertiesCount;
            ViewBag.ActiveAgreementsCount = activeContractsCount;

            return View();
        }

        // GET: Tenant/PaymentNotifications - UPDATED
        public async Task<IActionResult> PaymentNotifications()
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var notifications = await _notifService.GetTenantNotificationsAsync(tenantId.Value);
            return View(notifications);
        }

        // GET: Tenant/OpenNotification/{id} - UPDATED
        public async Task<IActionResult> OpenNotification(int id)
        {
            await _notifService.MarkAsSeenAsync(id);

            var tenantId = HttpContext.Session.GetInt32("UserId");
            var notification = await _notifService.GetNotificationByIdAsync(id,tenantId.Value);

            if (notification == null) return NotFound();

            return RedirectToAction("ConfirmPayment", "Booking", new { paymentId = notification.PaymentId });
        }

        // GET: Tenant/PaymentHistory - UPDATED
        [HttpGet]
        public async Task<IActionResult> PaymentHistory()
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var history = await _paymentService.GetPaymentHistoryAsync(tenantId.Value);
            return View(history);
        }

        // GET: Tenant/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var bookings = await _rentalRequestRepo.GetRequestsForTenantAsync(tenantId.Value);
            var viewModels = new List<TenantBookingVM>();
            foreach (var booking in bookings)
            {
                if (booking.Property != null)
                {
                    var propertyDetails = await _propertyRepo.GetPropertyDetailsAsync(booking.Property.Id);
                    booking.Property.Images = propertyDetails?.Images;
                }

                var isEligible = await _reviewService.IsReviewEligibleAsync(tenantId.Value, booking.PropertyId);
                System.Console.WriteLine($"[DEBUG] Tenant: {tenantId.Value}, Property: {booking.PropertyId}, Eligible: {isEligible}");

                var vm = new TenantBookingVM
                {
                    Request = booking,
                    IsReviewEligible = isEligible
                };

                if (vm.IsReviewEligible)
                {
                    vm.ExistingReviewId = await _reviewService.GetExistingReviewIdAsync(tenantId.Value, booking.PropertyId);
                    vm.HasExistingReview = vm.ExistingReviewId.HasValue;
                    System.Console.WriteLine($"[DEBUG] Tenant: {tenantId.Value}, Property: {booking.PropertyId}, ExistingReviewId: {vm.ExistingReviewId}");
                }

                viewModels.Add(vm);
            }

            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int requestId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var booking = await _rentalRequestRepo.GetRequestByIdAsync(requestId);

            if (booking == null || booking.TenantId != tenantId.Value)
            {
                TempData["Error"] = "Booking not found or you don't have permission to cancel this booking.";
                return RedirectToAction("MyBookings");
            }

            if (booking.StatusId == 3)
            {
                TempData["Error"] = "This booking is already cancelled.";
                return RedirectToAction("MyBookings");
            }

            if (booking.StatusId == 4)
            {
                TempData["Error"] = "Completed bookings cannot be cancelled.";
                return RedirectToAction("MyBookings");
            }

            booking.StatusId = 3;
            await _rentalRequestRepo.UpdateRequestAsync(booking);

            TempData["Success"] = "Booking cancelled successfully!";
            return RedirectToAction("MyBookings");
        }

        public async Task<IActionResult> BookingDetails(int id)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var booking = await _rentalRequestRepo.GetRequestByIdAsync(id);

            if (booking == null || booking.TenantId != tenantId.Value)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("MyBookings");
            }

            if (booking.Property != null)
            {
                var propertyDetails = await _propertyRepo.GetPropertyDetailsAsync(booking.Property.Id);
                booking.Property.Images = propertyDetails?.Images;
            }

            return View(booking);
        }
    }
}