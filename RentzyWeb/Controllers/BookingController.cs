using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.Services;
using Rentzy.DAL.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RentzyWeb.Controllers
{
    public class BookingController : Controller
    {
        private readonly ITenantBookingService _bookingService;
        private readonly TenantPaymentService _paymentService;
        private readonly ReviewService _reviewService;

        public BookingController(ITenantBookingService bookingService, TenantPaymentService paymentService, ReviewService reviewService)
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await _bookingService.GetPropertyDetailsAsync(id);
            if (model == null) return NotFound();

            var tenantId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserType");

            if (tenantId.HasValue && userRole == "Tenant")
            {
                model.IsReviewEligible = await _reviewService.IsReviewEligibleAsync(tenantId.Value, id);
                if (model.IsReviewEligible)
                {
                    model.ExistingReviewId = await _reviewService.GetExistingReviewIdAsync(tenantId.Value, id);
                    model.HasExistingReview = model.ExistingReviewId.HasValue;
                }
            }

            return View(model);
        }

        // REQUEST GET - Show booking request form
        public async Task<IActionResult> Request(int propertyId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var property = await _bookingService.GetPropertyDetailsAsync(propertyId);
            if (property == null) return NotFound();

            if (!property.IsApproved)
            {
                TempData["ErrorMessage"] = "This property is not approved for renting yet.";
                return RedirectToAction("Details", new { id = propertyId });
            }

            var bookedDates = await _bookingService.GetBookedDatesAsync(propertyId);

            ViewBag.PropertyId = propertyId;
            ViewBag.PropertyTitle = property.Title;
            ViewBag.PropertyImage = property.Images?.FirstOrDefault()?.ImageUrl;
            ViewBag.BookedDates = bookedDates?.Select(d => d.ToString("yyyy-MM-dd")).ToList() ?? new System.Collections.Generic.List<string>();

            return View();
        }

        // REQUEST POST - Submit booking request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(int propertyId, DateTime startDate, DateTime endDate)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var property = await _bookingService.GetPropertyDetailsAsync(propertyId);
            if (property == null) return NotFound();

            if (!property.IsApproved)
            {
                TempData["ErrorMessage"] = "This property is not approved for renting.";
                return RedirectToAction("Details", new { id = propertyId });
            }

            var bookedDates = await _bookingService.GetBookedDatesAsync(propertyId);
            var selectedDates = new System.Collections.Generic.List<DateTime>();

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                selectedDates.Add(date);
            }

            var conflictingDates = selectedDates.Where(d => bookedDates.Contains(d.Date)).ToList();

            if (conflictingDates.Any())
            {
                TempData["ErrorMessage"] = $"Cannot book: Selected dates include booked periods ({string.Join(", ", conflictingDates.Select(d => d.ToString("yyyy-MM-dd")))}). Please choose different dates.";
                return RedirectToAction("Request", new { propertyId = propertyId });
            }

            var request = new PropertyRentalRequest
            {
                PropertyId = propertyId,
                TenantId = tenantId.Value,
                StartDate = startDate,
                EndDate = endDate,
                StatusId = 1, // PENDING
                RequestedAt = DateTime.Now
            };

            await _bookingService.CreateRentalRequestAsync(request);

            TempData["SuccessMessage"] = "Rental request sent! Waiting for landlord approval.";
            return RedirectToAction("Details", new { id = propertyId });
        }

        // PAYMENT GET - Show payment information
        public async Task<IActionResult> Payment(int requestId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var model = await _bookingService.GetPaymentInfoAsync(requestId);
            if (model == null) return NotFound();

            return View(model);
        }

        // PAYMENT CONFIRMATION GET - Show payment confirmation page
        public async Task<IActionResult> ConfirmPayment(int paymentId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var payment = await _bookingService.GetPaymentByIdAsync(paymentId);
            if (payment == null) return NotFound();

            // FIX: Get the booking to check tenant authorization
            var booking = await _bookingService.GetBookingByPaymentIdAsync(paymentId);
            if (booking == null || booking.TenantId != tenantId.Value)
                return Forbid();

            // Check if payment is already paid (StatusId = 2)
            if (payment.StatusId == 2) // Paid
            {
                TempData["Info"] = "This payment has already been processed.";
                return RedirectToAction("Receipt", new { paymentId = paymentId });
            }

            // Check if payment is failed (StatusId = 3)
            if (payment.StatusId == 3) // Failed
            {
                TempData["ErrorMessage"] = "This payment has failed. Please try again.";
            }

            return View(payment);
        }

        // PAYMENT CONFIRMATION POST - Process the payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int paymentId, string paymentMethod)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            try
            {
                var payment = await _bookingService.GetPaymentByIdAsync(paymentId);
                if (payment == null) return NotFound();

                // FIX: Get the booking to check tenant authorization
                var booking = await _bookingService.GetBookingByPaymentIdAsync(paymentId);
                if (booking == null || booking.TenantId != tenantId.Value)
                    return Forbid();

                // Check if payment is already paid (StatusId = 2)
                if (payment.StatusId == 2)
                {
                    TempData["ErrorMessage"] = "This payment has already been processed.";
                    return RedirectToAction("PaymentHistory", "Tenant");
                }

                // Process the payment with selected method
                var success = await _paymentService.ProcessPaymentAsync(paymentId, paymentMethod);
                if (success)
                {
                    // TODO: Implement payment status update in service
                    // For now, we'll assume the payment service handles status updates
                    TempData["SuccessMessage"] = $"Payment of {payment.Amount:C} completed successfully using {GetPaymentMethodName(paymentMethod)}!";
                    return RedirectToAction("Receipt", new { paymentId = paymentId });
                }
                else
                {
                    // TODO: Implement payment status update in service  
                    TempData["ErrorMessage"] = "Payment processing failed. Please try again.";
                    return RedirectToAction("ConfirmPayment", new { paymentId = paymentId });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Payment error: {ex.Message}";
                return RedirectToAction("ConfirmPayment", new { paymentId = paymentId });
            }
        }

        // RECEIPT - Show payment receipt
        public async Task<IActionResult> Receipt(int paymentId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            try
            {
                var payment = await _bookingService.GetPaymentByIdAsync(paymentId);
                if (payment == null) return NotFound();

                // FIX: Get the booking to check tenant authorization
                var booking = await _bookingService.GetBookingByPaymentIdAsync(paymentId);
                if (booking == null || booking.TenantId != tenantId.Value)
                    return Forbid();

                if (payment.StatusId != 2) // Not paid (StatusId = 2 is Paid)
                {
                    TempData["ErrorMessage"] = "Payment not completed. Please complete the payment first.";
                    return RedirectToAction("ConfirmPayment", new { paymentId = paymentId });
                }

                // Get receipt data
                var receipt = await _paymentService.GetReceiptAsync(paymentId);
                return View(receipt);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating receipt: {ex.Message}";
                return RedirectToAction("PaymentHistory", "Tenant");
            }
        }

        // UPDATED PAYMENT METHOD - Fixed to properly handle payment flow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int paymentId)
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
            if (payment.StatusId == 2) // Already paid
            {
                TempData["ErrorMessage"] = "This payment has already been processed.";
                return RedirectToAction("Receipt", new { paymentId = paymentId });
            }

            if (payment.StatusId == 3) // Failed
            {
                TempData["Info"] = "Previous payment failed. You can try again.";
            }

            return RedirectToAction("ConfirmPayment", new { paymentId = paymentId });
        }

        // NEW: Get pending payments for notifications
        public async Task<IActionResult> GetPendingPayments()
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return Json(new { error = "Not authenticated" });

            try
            {
                var pendingPayments = await _paymentService.GetPendingPaymentsAsync(tenantId.Value);
                return Json(pendingPayments);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // NEW: Direct payment from payment ID
        public async Task<IActionResult> DirectPayment(int paymentId)
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
                return RedirectToAction("Receipt", new { paymentId = paymentId });
            }

            if (payment.StatusId == 3) // Failed
            {
                TempData["Info"] = "Previous payment failed. You can try again.";
            }

            return RedirectToAction("ConfirmPayment", new { paymentId = paymentId });
        }

        private string GetPaymentMethodName(string method)
        {
            return method switch
            {
                "credit_card" => "Credit Card",
                "paypal" => "PayPal",
                "bank_transfer" => "Bank Transfer",
                "digital_wallet" => "Digital Wallet",
                "not_paid" => "Not Paid",
                _ => "Unknown Method"
            };
        }
    }
}