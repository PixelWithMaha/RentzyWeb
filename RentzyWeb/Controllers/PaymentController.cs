using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.DTOs;
using Rentzy.BLL.Services;
using Rentzy.Web.Authorization;
using System.Threading.Tasks;

namespace Rentzy.Web.Controllers
{
    [AuthorizeRole("Landlord")]
    public class PaymentController : Controller
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // GET: /Payment/Index → now PaymentsView
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var landlordId = HttpContext.Session.GetInt32("UserId");
            if (landlordId == null) return RedirectToAction("Login", "Account");

            var payments = await _paymentService.GetPaymentsForLandlordAsync(landlordId.Value);
            return View("PaymentsView", payments); // renamed view
        }

        // GET: /Payment/Details/{id} → now PaymentsDetails
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetPaymentDetailsAsync(id);
            if (payment == null) return NotFound();

            return View("PaymentsDetails", payment); // renamed view
        }

        // POST: Send reminder for pending payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendReminder(int paymentId)
        {
            var success = await _paymentService.SendReminderAsync(paymentId);
            TempData["SuccessMessage"] = success
                ? "Payment reminder sent to tenant."
                : "Failed to send reminder.";
            return RedirectToAction("Index");
        }
    }
}
