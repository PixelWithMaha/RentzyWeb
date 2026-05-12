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

        public BookingController(ITenantBookingService bookingService, TenantPaymentService paymentService)
        {
            _bookingService = bookingService;
            _paymentService = paymentService;
        }

        // DETAILS - View property details
        public async Task<IActionResult> Details(int id)
        {
            var model = await _bookingService.GetPropertyDetailsAsync(id);
            if (model == null) return NotFound();
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

        // CONFIRM PAYMENT GET - Show payment confirmation page
        public async Task<IActionResult> ConfirmPayment(int paymentId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var payment = await _bookingService.GetPaymentByIdAsync(paymentId);
            if (payment == null) return NotFound();

            var booking = await _bookingService.GetBookingByPaymentIdAsync(paymentId);
            if (booking == null || booking.TenantId != tenantId.Value)
                return Forbid();

            // If already paid, go directly to receipt
            if (payment.StatusId == 2 && payment.PaidAt != null)
            {
                TempData["Info"] = "This payment has already been processed.";
                return RedirectToAction("Receipt", new { paymentId = paymentId });
            }

            // If failed, show message but still allow retry
            if (payment.StatusId == 3)
            {
                TempData["ErrorMessage"] = "This payment previously failed. Please try again.";
            }

            return View(payment);
        }

        // CONFIRM PAYMENT POST - Process the payment
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

                var booking = await _bookingService.GetBookingByPaymentIdAsync(paymentId);
                if (booking == null || booking.TenantId != tenantId.Value)
                    return Forbid();

                // Already paid — redirect to receipt
                if (payment.StatusId == 2 && payment.PaidAt != null)
                {
                    TempData["Info"] = "This payment has already been processed.";
                    return RedirectToAction("Receipt", new { paymentId = paymentId });
                }

                // Process the payment
                var success = await _paymentService.ProcessPaymentAsync(paymentId, paymentMethod);

                if (success)
                {
                    TempData["SuccessMessage"] = $"Payment of {payment.Amount:C} completed successfully using {GetPaymentMethodName(paymentMethod)}!";
                    return RedirectToAction("Receipt", new { paymentId = paymentId });
                }
                else
                {
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

                var booking = await _bookingService.GetBookingByPaymentIdAsync(paymentId);
                if (booking == null || booking.TenantId != tenantId.Value)
                    return Forbid();

                // FIX: Must check BOTH StatusId == 2 AND PaidAt is not null
                if (payment.StatusId != 2 || payment.PaidAt == null)
                {
                    TempData["ErrorMessage"] = "Payment not completed. Please complete the payment first.";
                    return RedirectToAction("ConfirmPayment", new { paymentId = paymentId });
                }

                var receipt = await _paymentService.GetReceiptAsync(paymentId);
                return View(receipt);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating receipt: {ex.Message}";
                return RedirectToAction("PaymentHistory", "Tenant");
            }
        }

        // PAY POST - Redirect to ConfirmPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int paymentId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var payment = await _bookingService.GetPaymentByIdAsync(paymentId);
            if (payment == null) return NotFound();

            var booking = await _bookingService.GetBookingByPaymentIdAsync(paymentId);
            if (booking == null || booking.TenantId != tenantId.Value)
                return Forbid();

            // Already paid — go to receipt
            if (payment.StatusId == 2 && payment.PaidAt != null)
            {
                TempData["Info"] = "This payment has already been processed.";
                return RedirectToAction("Receipt", new { paymentId = paymentId });
            }

            if (payment.StatusId == 3)
            {
                TempData["Info"] = "Previous payment failed. You can try again.";
            }

            return RedirectToAction("ConfirmPayment", new { paymentId = paymentId });
        }

        // GET PENDING PAYMENTS - Returns JSON for notifications
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

        // DIRECT PAYMENT - Redirect to ConfirmPayment from a paymentId directly
        public async Task<IActionResult> DirectPayment(int paymentId)
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var payment = await _bookingService.GetPaymentByIdAsync(paymentId);
            if (payment == null) return NotFound();

            var booking = await _bookingService.GetBookingByPaymentIdAsync(paymentId);
            if (booking == null || booking.TenantId != tenantId.Value)
                return Forbid();

            if (payment.StatusId == 2 && payment.PaidAt != null)
            {
                TempData["Info"] = "This payment has already been processed.";
                return RedirectToAction("Receipt", new { paymentId = paymentId });
            }

            if (payment.StatusId == 3)
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
