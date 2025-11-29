using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.Services;
using Rentzy.Web.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace RentzyWeb.Controllers
{
    [AuthorizeRole("Tenant")] // Only Tenants can access this controller

    public class TenantController : Controller
    {
        private readonly PaymentNotificationService _notifService;
        private readonly TenantPaymentService _Service;

        public TenantController(PaymentNotificationService notifService, TenantPaymentService service)
        {
            _notifService = notifService;
            _Service = service;
        }

        // Dashboard action you already have
        //[HttpGet]
        //public IActionResult DashboardStats()
        //{
        //    var userName = HttpContext.Session.GetString("UserName");
        //    var userEmail = HttpContext.Session.GetString("UserEmail");

        //    ViewBag.UserName = userName;
        //    ViewBag.UserEmail = userEmail;

        //    return View();

        //}
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var rentedPropertiesCount = await _Service.GetRentedPropertiesCountAsync(tenantId.Value);
            var activeContractsCount = await _Service.GetActiveContractsCountAsync(tenantId.Value);

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.MyPropertiesCount = rentedPropertiesCount;
            ViewBag.ActiveAgreementsCount = activeContractsCount;

            return View();
        }

        // ✅ This is the action for payment notifications
        public async Task<IActionResult> PaymentNotifications()
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var notifications = await _notifService.GetTenantNotificationsAsync(tenantId.Value);
            return View(notifications);
        }

        // ✅ Open specific notification and redirect to payment
        public async Task<IActionResult> OpenNotification(int id)
        {
            await _notifService.MarkAsSeenAsync(id);

            var tenantId = HttpContext.Session.GetInt32("UserId");
            var notifications = await _notifService.GetTenantNotificationsAsync(tenantId.Value);
            var notif = notifications.FirstOrDefault(n => n.Id == id);
            if (notif == null) return NotFound();

            return RedirectToAction("Payment", "Booking", new { paymentId = notif.PaymentId });
        }
        [HttpGet]
        public async Task<IActionResult> PaymentHistory()
        {
            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            var history = await _Service.GetPaidPaymentsAsync(tenantId.Value); // only paid payments
            return View(history);
        }

        //[HttpGet]
        //public async Task<IActionResult> Dashboard([FromServices] TenantPaymentService paymentService)
        //{
        //    var tenantId = HttpContext.Session.GetInt32("UserId");
        //    if (tenantId == null) return RedirectToAction("Login", "Account");

        //    var rentedPropertiesCount = await paymentService.GetRentedPropertiesCountAsync(tenantId.Value);
        //    var activeContractsCount = await paymentService.GetActiveContractsCountAsync(tenantId.Value);

        //    ViewBag.UserName = HttpContext.Session.GetString("UserName");
        //    ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
        //    ViewBag.MyPropertiesCount = rentedPropertiesCount;
        //    ViewBag.ActiveAgreementsCount = activeContractsCount;

        //    return View();
        //}

    }
}
