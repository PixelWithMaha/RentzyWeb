using Microsoft.AspNetCore.Mvc;
using Rentzy.Web.Authorization;

namespace Rentzy.Web.Controllers
{
    [AuthorizeRole("Landlord")] // Only Landlords can access this controller
    public class LandlordController : Controller
    {
        [HttpGet]
        public IActionResult Dashboard()
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            ViewBag.UserName = userName;
            ViewBag.UserEmail = userEmail;

            return View();
        }
    }
}