using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentzy.BLL.DTOs; // for PaymentDTO if needed
using Rentzy.BLL.Services;
using Rentzy.DAL.Repository;
using Rentzy.DAL.Context;
using System;
using System.Threading.Tasks;

namespace RentzyWeb.Controllers
{
    public class BookingController : Controller
    {
       // private readonly IPropertyService _service;
        private readonly PropertyService _service;


        public BookingController()
        {
            //_service = service;
            var options = new DbContextOptionsBuilder<RentzyDBContext>()
        .UseSqlServer("Server=localhost\\SQLEXPRESS;Database=RentzyDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False")
        .Options;

            var db = new RentzyDBContext(options);

            var landlordRepo = new LandlordRepository(db);
            var propertyRepo = new PropertyRepository(db);

            _service = new PropertyService(landlordRepo, propertyRepo);
        }

        // DETAILS: show full property page (uses existing PropertyDTO)
        public async Task<IActionResult> Details(int id)
        {
            var model = await _service.GetPropertyDetailsAsync(id); // returns PropertyDTO
            if (model == null) return NotFound();
            return View(model);
        }

        // CREATE RENTAL REQUEST: tenant clicks Book -> create PropertyRentalRequest record
        public async Task<IActionResult> Request(int propertyId)
        {
            // ensure session contains UserId
            //var userIdString = HttpContext.Session.GetString("UserId");
            //if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

           // int tenantId = int.Parse(userIdString);
           // int requestId = await _service.CreateRentalRequestAsync(tenantId, propertyId);

            var tenantId = HttpContext.Session.GetInt32("UserId");
            if (tenantId == null) return RedirectToAction("Login", "Account");

            // Use tenantId.Value below
            int requestId = await _service.CreateRentalRequestAsync(tenantId.Value, propertyId);


            // redirect to payment page for that request
            return RedirectToAction(nameof(Payment), new { requestId });
        }

        // PAYMENT (GET): show payment page for a rental request
        public async Task<IActionResult> Payment(int requestId)
        {
            var model = await _service.GetPaymentInfoAsync(requestId); // returns PaymentDTO
            if (model == null) return NotFound();
            return View(model);
        }

        // PROCESS PAYMENT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int requestId, decimal amount)
        {
            // you may want to check amount server-side in real app
            await _service.ProcessPaymentAsync(requestId, amount);
            return RedirectToAction(nameof(Receipt), new { requestId });
        }

        // RECEIPT
        public IActionResult Receipt(int requestId)
        {
            ViewBag.RequestId = requestId;
            return View();
        }
    }
}
