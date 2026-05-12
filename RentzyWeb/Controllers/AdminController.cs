using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentzy.BLL.Services.ApprovalServices;
using Rentzy.DAL.Context;
using Rentzy.DAL.Models;
using Rentzy.Web.Authorization;
using Rentzy.Web.Models;

namespace Rentzy.Web.Controllers
{
    [AuthorizeRole("Admin")] // Only Admins can access this controller
    public class AdminController : Controller
    {
        private readonly RentzyDBContext _context;
        private readonly ILandlordApprovalService _approvalService;
        private readonly IPropertyApprovalRequestService _propertyService;
        private readonly IUserStatuses_service _statusService;
        private readonly Rentzy.BLL.Services.ReviewService _reviewService;

        public AdminController(RentzyDBContext context, ILandlordApprovalService approvalService, IPropertyApprovalRequestService rep, IUserStatuses_service statusService, Rentzy.BLL.Services.ReviewService reviewService)
        {
            _context = context;
            _approvalService = approvalService;
            _propertyService = rep;
            _statusService = statusService;
            _reviewService = reviewService;
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userEmail = HttpContext.Session.GetString("UserEmail");

           
            var totalUsers = _context.Users.Count();
            var ActiveLandlord = _context.UserStatuses
                .Include(x => x.User)  
                .Count(x => x.IsActive == true && EF.Property<string>(x.User, "Discriminator") == "Landlord");

            var totalPrpperties = _context.PropertyApprovalRequests.Count();

            var Activetenants = _context.UserStatuses
                .Include(x => x.User) 
                .Count(x => x.IsActive == true && EF.Property<string>(x.User, "Discriminator") == "Tenant");


            ViewBag.TotalUsers = totalUsers;
            ViewBag.ActiveLandlords = ActiveLandlord;
            ViewBag.TotalProperties = totalPrpperties;
            ViewBag.UserName = userName;
            ViewBag.UserEmail = userEmail;
            ViewBag.ActiveTenants = Activetenants;
            
            // Fetch real-time review total asynchronously (force Wait/Result or convert method? Dashboard is not async but we can make it async)
            // Wait, Dashboard is [HttpGet] public IActionResult Dashboard().
            // Let's change Dashboard to be async Task<IActionResult>
            ViewBag.TotalReviews = _reviewService.GetTotalReviewsCountAsync().Result; 

            // Recent users (last 2 days)
            ViewBag.RecentUsers = _context.Users
                .Where(u => u.CreatedAt >= DateTime.Now.AddDays(-2))
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToList();

            // Pending landlord approvals
            ViewBag.PendingActions = _context.LandlordApprovals
                .Where(x => x.ApprovalStatusId == 1) // Pending
                .Take(5)
                .Select(x => new {
                    LandlordName = (x.Landlord.FirstName ?? "") + (string.IsNullOrWhiteSpace(x.Landlord.LastName) ? "" : " " + x.Landlord.LastName),
                    x.Id
                })
                .ToList();

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

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _context.Users.ToListAsync();

            var vm = new List<ManageUserVM>();

            foreach (var u in users)
            {
                var status = await _statusService.GetStatusAsync(u.Id);

                if( u.Role != "Admin")
                {
                    vm.Add(new ManageUserVM
                    {
                        UserId = u.Id,
                        FullName = $"{u.FirstName} {u.LastName}",
                        Email = u.Email,
                        IsActive = status.IsActive,
                        IsDeleted = status.IsDeleted
                    });
                }
            }

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> BlockUser(int userId)
        {
            await _statusService.BlockUserAsync(userId);
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUser(int userId)
        {
            await _statusService.UnblockUserAsync(userId);
            return RedirectToAction("ManageUsers");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var vm = new EditUserVM
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.Phone
            };
            return View(vm); // yahan Edit.cshtml load hogi
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
                return NotFound();

            // Update fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Phone = model.PhoneNumber;

            await _context.SaveChangesAsync();

            TempData["Success"] = "User updated successfully.";

            return RedirectToAction("ManageUsers", "Admin");
        }

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
        public async Task<IActionResult> EditLandlordApprovalStatus(int id)
        {
            var request = await _context.LandlordApprovals
                .Include(x => x.Landlord)
                .Include(x => x.ApprovalStatus)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null) return NotFound();

            ViewBag.Statuses = await _context.ApprovalStatuses.ToListAsync();
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLandlordApprovalStatus(int id, int statusId, string notes)
        {
            var request = await _context.LandlordApprovals
                .Include(x => x.Landlord)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (request == null) return NotFound();

            request.ApprovalStatusId = statusId;
            if (statusId == ApprovalStatusConstants.Approved)
            {
                request.Landlord.IsVerified =true;
            }
            else
                request.Landlord.IsVerified = false;
            request.AdminNotes = notes;
            request.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("LandlordApprovals");
        }

        [HttpGet]
        public async Task<IActionResult> EditPropertyStatus(int id)
        {
            var req = await _context.PropertyApprovalRequests
                .Include(x => x.property)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (req == null)
                return NotFound();

            return View(req);
        }

        [HttpPost]
        public async Task<IActionResult> EditPropertyStatus(int id, int statusId)
        {
            var req = await _context.PropertyApprovalRequests.FindAsync(id);
            if (req == null) return NotFound();

            req.StatusId = statusId;
            req.ReviewedAt = DateTime.UtcNow;

            _context.PropertyApprovalRequests.Update(req);
            await _context.SaveChangesAsync();

            return RedirectToAction("PropertyApprovals");
        }



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

        // ============================================================================================
        // Review Moderation
        // ============================================================================================

        [HttpGet]
        public async Task<IActionResult> ManageReviews()
        {
            var reviews = await _reviewService.GetReviewsForAdminAsync();
            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                await _reviewService.AdminDeleteReviewAsync(id);
                TempData["Success"] = "Review has been forcefully redacted from the ecosystem.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to redact review: " + ex.Message;
            }

            return RedirectToAction(nameof(ManageReviews));
        }
    }
}