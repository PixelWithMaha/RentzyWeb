using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Rentzy.Web.Authorization
{
    /// <summary>
    /// Custom authorization attribute to check if user is logged in via session
    /// Usage: [AuthorizeSession]
    /// </summary>
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");

            // If user is not logged in, redirect to login
            if (userId == null)
            {
                var controller = context.Controller as Controller;
                if (controller != null)
                {
                    controller.TempData["ErrorMessage"] = "Please login to access this page.";
                }
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Custom authorization attribute to check user role
    /// Usage: [AuthorizeRole("Landlord")] or [AuthorizeRole("Admin", "Landlord")]
    /// </summary>
    public class AuthorizeRoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UserId");
            var userType = context.HttpContext.Session.GetString("UserType");

            // Check if user is logged in
            if (userId == null)
            {
                var controller = context.Controller as Controller;
                if (controller != null)
                {
                    controller.TempData["ErrorMessage"] = "Please login to access this page.";
                }
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Check if user has the required role
            if (string.IsNullOrEmpty(userType) || !_allowedRoles.Contains(userType))
            {
                // User doesn't have permission - redirect to home with error
                var controller = context.Controller as Controller;
                if (controller != null)
                {
                    controller.TempData["ErrorMessage"] = $"Access Denied. This page is only for {string.Join(" or ", _allowedRoles)} users.";
                }
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}