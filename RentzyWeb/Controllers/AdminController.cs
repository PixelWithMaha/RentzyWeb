using Microsoft.AspNetCore.Mvc;
using Rentzy.BLL.Services.ApprovalServices;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using Rentzy.Web.Authorization;

namespace Rentzy.Web.Controllers
{
    [AuthorizeRole("Admin")] // Only Admins can access this controller
    public class AdminController : Controller
    {
        private readonly RentzyDBContext _context;
        private readonly ILandlordApprovalService _approvalService;
        private readonly IPropertyApprovalRequestService _propertyService;

        public AdminController(RentzyDBContext context, ILandlordApprovalService approvalService, IPropertyApprovalRequestService rep)
        {
            _context = context;
            _approvalService = approvalService;
            _propertyService = rep;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var totalUsers = _context.Users.Count();   
            var ActiveLandlord = _context.LandlordApprovals.Count(x => x.ApprovalStatusId == 2); 
            var totalPrpperties= _context.PropertyApprovalRequests.Count(x => x.StatusId == 2); 
            


            ViewBag.TotalUsers = totalUsers;
            ViewBag.ActiveLandlords = ActiveLandlord;
            ViewBag.TotalProperties = totalPrpperties;
            ViewBag.UserName = userName;
            ViewBag.UserEmail = userEmail;

            return View();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        //================================================================================================
        //  Manage Users
        //=============================================================================================



        //================================================================================================
        //  Property Approvals
        //=============================================================================================

        [HttpGet]
        public async Task<IActionResult> PropertyApprovals(int? statusId)
        {
            // default: show pending
            if (!statusId.HasValue) statusId = ApprovalStatusConstants.Pending;

            var approvals = await _propertyService.GetApprovalsByStatusAsync(statusId.Value);

            ViewBag.CurrentStatusId = statusId.Value;
            var statuses = _context.ApprovalStatuses.ToList();
            ViewBag.ApprovalStatuses = statuses;

            return View(approvals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProperty(int id, string? notes)
        {
            var adminId = GetCurrentAdminUserId(); // implement as per your auth/session
            try
            {
                await _propertyService.ApproveAsync(id, adminId, notes);
                TempData["Success"] = "Property approved successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("PropertyApprovals");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectProperty(int id, string? notes)
        {
            var adminId = GetCurrentAdminUserId();
            try
            {
                await _propertyService.RejectAsync(id, adminId, notes);
                TempData["Success"] = "Property rejected.";

            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("PropertyApprovals");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPropertyRequest(int id, string? notes)
        {
            var adminId = GetCurrentAdminUserId();
            try
            {
                await _propertyService.CancelAsync(id, adminId, notes);
                TempData["Success"] = "Approval request cancelled.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("PropertyApprovals");
        }

        [HttpGet]
        public async Task<IActionResult> PropertyApprovalDetails(int id)
        {
            var Approval = await _propertyService.GetByIdAsync(id);
            if (Approval == null) return NotFound();
            
                return View(Approval);
        }
      

        //================================================================================================
        //  Landlord Approvals
        //=============================================================================================


        // New: list approvals (filter optional)
        [HttpGet]
        public async Task<IActionResult> LandlordApprovals(int? statusId)
        {
            // default: show pending
            if (!statusId.HasValue) statusId = ApprovalStatusConstants.Pending;

            var approvals = await _approvalService.GetApprovalsByStatusAsync(statusId.Value);

            ViewBag.CurrentStatusId = statusId.Value;
            // Pass status lookup if you want to show status names in view
            var statuses = _context.ApprovalStatuses.ToList();
            ViewBag.ApprovalStatuses = statuses;

            return View(approvals); // LandlordApprovals.cshtml expecting List<LandlordApproval>
        }

        [HttpGet]
        public async Task<IActionResult> LandlordApprovalDetails(int id)
        {
            var approval = await _approvalService.GetByIdAsync(id);
            if (approval == null) return NotFound();

            return View(approval); // optional details view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLandlord(int id, string? notes)
        {
            var adminId = GetCurrentAdminUserId(); // implement as per your auth/session
            try
            {
                await _approvalService.ApproveAsync(id, adminId, notes);
                TempData["Success"] = "Landlord approved successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("LandlordApprovals");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectLandlord(int id, string? notes)
        {
            var adminId = GetCurrentAdminUserId();
            try
            {
                await _approvalService.RejectAsync(id, adminId, notes);
                TempData["Success"] = "Landlord rejected.";

            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("LandlordApprovals");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelLandlordApproval(int id, string? notes)
        {
            var adminId = GetCurrentAdminUserId();
            try
            {
                await _approvalService.CancelAsync(id, adminId, notes);
                TempData["Success"] = "Approval request cancelled.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("LandlordApprovals");
        }

        // Helper: adjust according to your session/auth scheme
        private int GetCurrentAdminUserId()
        {
            var idString = HttpContext.Session.GetString("Id");
            if (int.TryParse(idString, out var id)) return id;
            // fallback: admin id 0 (or throw)
            return 0;
        }
    }
}