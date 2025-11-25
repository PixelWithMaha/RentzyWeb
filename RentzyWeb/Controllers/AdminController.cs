using Microsoft.AspNetCore.Mvc;
using Rentzy.Web.Authorization;

namespace Rentzy.Web.Controllers
{
    [AuthorizeRole("Admin")] // Only Admins can access this controller
    public class AdminController : Controller
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

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }
    }
}