using Microsoft.AspNetCore.Mvc;
using Rentzy.Web.Authorization;


namespace Rentzy.Web.Controllers
{
    [AuthorizeRole("Tenant")] // Only Tenants can access this controller
    public class TenantController : Controller
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